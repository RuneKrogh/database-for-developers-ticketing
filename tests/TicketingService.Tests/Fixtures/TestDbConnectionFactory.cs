using Npgsql;
using TicketingService.Infrastructure.Database;

namespace TicketingService.Tests.Fixtures;

public class TestDbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public NpgsqlConnection Create() => new(connectionString);
}
