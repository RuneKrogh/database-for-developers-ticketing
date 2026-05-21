using Dapper;
using TicketingService.Domain;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Infrastructure.Repositories;

public class BookingRepository(IDbConnectionFactory connectionFactory)
{
    public async Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId)
    {
        await using var connection = connectionFactory.Create();
        return await connection.QueryAsync<Booking>(
            "SELECT * FROM bookings WHERE event_id = @EventId ORDER BY booked_at",
            new { EventId = eventId });
    }
}
