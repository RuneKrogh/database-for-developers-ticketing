using Npgsql;

namespace TicketingService.Infrastructure.Database;

public interface IDbConnectionFactory
{
    NpgsqlConnection Create();
}
