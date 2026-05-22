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

        // All tasks wait behind a shared start signal so they are released
        // simultaneously, increasing the likelihood of overlap at the database level.
        var startSignal = new TaskCompletionSource();

        var tasks = Enumerable.Range(1, 10)
            .Select(userId => Task.Run(async () =>
            {
                await startSignal.Task;
                return await TryBookAsync(strategy, eventId, userId);
            }))
            .ToList();

        startSignal.SetResult();

        var results = await Task.WhenAll(tasks);

        var successes   = results.Count(r => r.Success);
        var dbExceptions = results.Count(r => r.Exception is PostgresException);

        // The failure mode is unhandled PostgresExceptions from the CHECK constraint,
        // not clean NoSeatsAvailableExceptions. That is the bug the unsafe strategy demonstrates.
        Assert.True(dbExceptions > 0, "Expected unhandled database exceptions from concurrent unsafe bookings.");

        // The CHECK constraint prevents available_seats from going negative and the
        // decrement executes before the booking insert, so the numeric state ends up
        // correct by accident — but the workflow itself is uncontrolled.
        Assert.Equal(0, await fixture.GetAvailableSeatsAsync(eventId));
        Assert.Equal(successes, await fixture.GetBookingCountAsync(eventId));
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
