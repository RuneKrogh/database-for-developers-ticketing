using Dapper;

namespace TicketingService.Infrastructure.Database;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        var sql = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Database", "init.sql"));

        await using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(sql);
    }
}
