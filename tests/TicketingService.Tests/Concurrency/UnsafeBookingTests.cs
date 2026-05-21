using Npgsql;
using TicketingService.Application.Strategies;
using TicketingService.Tests.Fixtures;

namespace TicketingService.Tests.Concurrency;

[Collection("Database")]
public class UnsafeBookingTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task ConcurrentBookings_WithOneSeatAvailable_ViolatesCheckConstraint()
    {
        var eventId = await fixture.CreateEventAsync("Unsafe Test Event", 1);
        var strategy = new UnsafeBookingStrategy(fixture.ConnectionFactory);

        // Task.Run forces each booking onto a separate thread pool thread so the
        // reads genuinely race rather than serialising on a single async thread.
        var tasks = Enumerable.Range(1, 10)
            .Select(userId => Task.Run(() => TryBookAsync(strategy, eventId, userId)));

        var results = await Task.WhenAll(tasks);

        // All 10 tasks pass the application-level check before any write commits.
        // The CHECK constraint then rejects the updates that would push available_seats
        // below zero. The failure mode is unhandled PostgresExceptions, not clean
        // NoSeatsAvailableExceptions — that is the bug the unsafe strategy demonstrates.
        var dbExceptions = results.Count(r => r.Exception is PostgresException);
        Assert.True(dbExceptions > 0, "Expected unhandled database exceptions from concurrent unsafe bookings.");
    }

    private static async Task<(bool Success, Exception? Exception)> TryBookAsync(
        IBookingStrategy strategy, int eventId, int userId)
    {
        try
        {
            await strategy.BookAsync(eventId, userId);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}
