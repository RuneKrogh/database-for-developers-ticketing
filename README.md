# Database for Developers — Ticketing Concurrency

A .NET 8 ticketing API demonstrating how overselling happens through race conditions and how to prevent it using different database concurrency strategies.

## Projects

| Project | Description |
|---------|-------------|
| `TicketingService` | ASP.NET Core Web API for managing events and bookings |
| `TicketingService.Tests` | xUnit integration tests |

## Concurrency Strategies

| Strategy | Mechanism | Failure Mode |
|----------|-----------|--------------|
| `unsafe` | No protection — plain SELECT then UPDATE | `PostgresException` (CHECK constraint) |
| `pessimistic` | `SELECT FOR UPDATE` — row-level lock for transaction duration | `NoSeatsAvailableException` |
| `optimistic` | Version column — conditional `UPDATE WHERE version = @Version` | `NoSeatsAvailableException` |
| `serializable` | `SERIALIZABLE` isolation — SSI detects write-write conflict, retries on `40001` | `NoSeatsAvailableException` |

## Running

```bash
docker compose up --build
```

| Endpoint | URL |
|----------|-----|
| API | http://localhost:5000 |

## Tests

```bash
dotnet test tests/TicketingService.Tests
```

The integration tests use Testcontainers to spin up a real PostgreSQL instance. Each concurrency test creates an event with one available seat, launches ten concurrent bookings, and verifies that exactly one succeeds.
