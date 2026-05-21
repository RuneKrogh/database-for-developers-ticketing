using Dapper;
using TicketingService.Domain;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Application.Strategies;

public class UnsafeBookingStrategy(IDbConnectionFactory connectionFactory) : IBookingStrategy
{
    public async Task<Booking> BookAsync(int eventId, int userId)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        // Read available seats — no lock, no transaction
        var availableSeats = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT available_seats FROM events WHERE id = @EventId",
            new { EventId = eventId });

        // Another request can pass this same check concurrently before either updates
        if (availableSeats <= 0)
            throw new NoSeatsAvailableException(eventId);

        await connection.ExecuteAsync(
            "UPDATE events SET available_seats = available_seats - 1 WHERE id = @EventId",
            new { EventId = eventId });

        return await connection.QuerySingleAsync<Booking>(
            "INSERT INTO bookings (event_id, user_id) VALUES (@EventId, @UserId) RETURNING *",
            new { EventId = eventId, UserId = userId });
    }
}
