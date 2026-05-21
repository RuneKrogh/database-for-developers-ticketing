using System.Data;
using Dapper;
using Npgsql;
using TicketingService.Domain;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Application.Strategies;

public class SerializableBookingStrategy(IDbConnectionFactory connectionFactory) : IBookingStrategy
{
    private const int MaxRetries = 3;

    public async Task<Booking> BookAsync(int eventId, int userId)
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                await using var connection = connectionFactory.Create();
                await connection.OpenAsync();

                // SERIALIZABLE is the strictest isolation level. PostgreSQL tracks all reads
                // and writes within the transaction and aborts it if executing them serially
                // would have produced a different result — no explicit locks needed.
                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

                var availableSeats = await connection.QuerySingleOrDefaultAsync<int>(
                    "SELECT available_seats FROM events WHERE id = @EventId",
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
            catch (PostgresException ex) when (ex.SqlState == "40001")
            {
                // Serialization failure: PostgreSQL detected that concurrent transactions
                // could not have executed serially. Retry with a fresh read.
                if (attempt == MaxRetries - 1)
                    throw new NoSeatsAvailableException(eventId);
            }
        }

        throw new NoSeatsAvailableException(eventId);
    }
}
