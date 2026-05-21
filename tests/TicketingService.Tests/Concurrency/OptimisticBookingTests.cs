using TicketingService.Application.Strategies;
using TicketingService.Tests.Fixtures;

namespace TicketingService.Tests.Concurrency;

[Collection("Database")]
public class OptimisticBookingTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task ConcurrentBookings_WithOneSeatAvailable_ExactlyOneSucceeds()
    {
        var eventId = await fixture.CreateEventAsync("Optimistic Test Event", 1);
        var strategy = new OptimisticBookingStrategy(fixture.ConnectionFactory);

        var tasks = Enumerable.Range(1, 10)
            .Select(userId => Task.Run(() => TryBookAsync(strategy, eventId, userId)));

        var results = await Task.WhenAll(tasks);

        var successes = results.Count(r => r.Success);
        var cleanFailures = results.Count(r => r.Exception is NoSeatsAvailableException);

        Assert.Equal(1, successes);
        Assert.Equal(9, cleanFailures);
        Assert.Equal(1, await fixture.GetBookingCountAsync(eventId));
        Assert.Equal(0, await fixture.GetAvailableSeatsAsync(eventId));
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
