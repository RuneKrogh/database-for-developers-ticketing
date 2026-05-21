using Dapper;
using TicketingService.Domain;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Application.Strategies;

public class OptimisticBookingStrategy(IDbConnectionFactory connectionFactory) : IBookingStrategy
{
    private const int MaxRetries = 3;

    public async Task<Booking> BookAsync(int eventId, int userId)
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            await using var connection = connectionFactory.Create();
            await connection.OpenAsync();

            // Read the current state — no lock needed
            var ev = await connection.QuerySingleOrDefaultAsync<Event>(
                "SELECT id, available_seats, version FROM events WHERE id = @EventId",
                new { EventId = eventId });

            if (ev is null || ev.AvailableSeats <= 0)
                throw new NoSeatsAvailableException(eventId);

            await using var transaction = await connection.BeginTransactionAsync();

            // Conditional update: only succeeds if the version has not changed since we read it.
            // If another transaction committed first, the version will have incremented and
            // this update will match 0 rows, signalling a conflict.
            var rowsUpdated = await connection.ExecuteAsync(
                """
                UPDATE events
                SET available_seats = available_seats - 1, version = version + 1
                WHERE id = @EventId AND version = @Version
                """,
                new { EventId = eventId, Version = ev.Version },
                transaction);

            if (rowsUpdated == 0)
            {
                await transaction.RollbackAsync();
                continue;
            }

            var booking = await connection.QuerySingleAsync<Booking>(
                "INSERT INTO bookings (event_id, user_id) VALUES (@EventId, @UserId) RETURNING *",
                new { EventId = eventId, UserId = userId },
                transaction);

            await transaction.CommitAsync();
            return booking;
        }

        throw new NoSeatsAvailableException(eventId);
    }
}
