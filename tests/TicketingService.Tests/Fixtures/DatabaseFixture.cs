using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public IDbConnectionFactory ConnectionFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        ConnectionFactory = new TestDbConnectionFactory(_postgres.GetConnectionString());
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        await ApplySchemaAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    public async Task<int> CreateEventAsync(string name, int seats)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        return await connection.QuerySingleAsync<int>(
            "INSERT INTO events (name, total_seats, available_seats) VALUES (@Name, @Seats, @Seats) RETURNING id",
            new { Name = name, Seats = seats });
    }

    public async Task<int> GetBookingCountAsync(int eventId)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        return await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM bookings WHERE event_id = @EventId",
            new { EventId = eventId });
    }

    public async Task<int> GetAvailableSeatsAsync(int eventId)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        return await connection.QuerySingleAsync<int>(
            "SELECT available_seats FROM events WHERE id = @Id",
            new { Id = eventId });
    }

    private async Task ApplySchemaAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS users (
                id    SERIAL PRIMARY KEY,
                name  TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS events (
                id               SERIAL PRIMARY KEY,
                name             TEXT NOT NULL,
                total_seats      INT  NOT NULL,
                available_seats  INT  NOT NULL CHECK (available_seats >= 0),
                version          INT  NOT NULL DEFAULT 0,
                created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS bookings (
                id        SERIAL PRIMARY KEY,
                event_id  INT NOT NULL REFERENCES events(id),
                user_id   INT NOT NULL REFERENCES users(id),
                booked_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            INSERT INTO users (name, email) VALUES
                ('Alice',   'alice@example.com'),
                ('Bob',     'bob@example.com'),
                ('Charlie', 'charlie@example.com'),
                ('Diana',   'diana@example.com'),
                ('Eve',     'eve@example.com'),
                ('Frank',   'frank@example.com'),
                ('Grace',   'grace@example.com'),
                ('Heidi',   'heidi@example.com'),
                ('Ivan',    'ivan@example.com'),
                ('Judy',    'judy@example.com')
            ON CONFLICT (email) DO NOTHING;
            """);
    }
}
