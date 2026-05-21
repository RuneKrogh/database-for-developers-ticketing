# Database for Developers — Ticketing Concurrency

A .NET 8 ticketing API demonstrating how overselling happens through race conditions and how to prevent it using different database concurrency strategies.

## Projects

| Project | Description |
|---------|-------------|
| `TicketingService` | ASP.NET Core Web API for managing events and bookings |
| `TicketingService.Tests` | xUnit integration tests |

## Running

```bash
docker compose up --build
```
