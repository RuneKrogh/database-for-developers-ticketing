using Dapper;
using TicketingService.Domain;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Infrastructure.Repositories;

public class EventRepository(IDbConnectionFactory connectionFactory)
{
    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        await using var connection = connectionFactory.Create();
        return await connection.QueryAsync<Event>("SELECT * FROM events ORDER BY id");
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<Event>(
            "SELECT * FROM events WHERE id = @Id", new { Id = id });
    }

    public async Task<Event> CreateAsync(string name, int totalSeats)
    {
        await using var connection = connectionFactory.Create();
        return await connection.QuerySingleAsync<Event>(
            """
            INSERT INTO events (name, total_seats, available_seats)
            VALUES (@Name, @TotalSeats, @TotalSeats)
            RETURNING *
            """,
            new { Name = name, TotalSeats = totalSeats });
    }
}
