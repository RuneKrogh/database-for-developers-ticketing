using Dapper;
using TicketingService.Domain;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Application.Strategies;

public class PessimisticBookingStrategy(IDbConnectionFactory connectionFactory) : IBookingStrategy
{
    public async Task<Booking> BookAsync(int eventId, int userId)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        // FOR UPDATE locks the event row until the transaction commits.
        // Concurrent requests block here and wait rather than reading stale data.
        var availableSeats = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT available_seats FROM events WHERE id = @EventId FOR UPDATE",
            new { EventId = eventId },
            transaction);

        if (availableSeats <= 0)
        {
            await transaction.RollbackAsync();
            throw new NoSeatsAvailableException(eventId);
        }

        await connection.ExecuteAsync(
            "UPDATE events SET available_seats = available_seats - 1 WHERE id = @EventId",
            new { EventId = eventId },
            transaction);

        var booking = await connection.QuerySingleAsync<Booking>(
            "INSERT INTO bookings (event_id, user_id) VALUES (@EventId, @UserId) RETURNING *",
            new { EventId = eventId, UserId = userId },
            transaction);

        await transaction.CommitAsync();
        return booking;
    }
}
