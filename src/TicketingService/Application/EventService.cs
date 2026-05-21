using TicketingService.Domain;
using TicketingService.Infrastructure.Repositories;

namespace TicketingService.Application;

public class EventService(EventRepository events)
{
    public Task<IEnumerable<Event>> GetAllAsync() =>
        events.GetAllAsync();

    public Task<Event?> GetByIdAsync(int id) =>
        events.GetByIdAsync(id);

    public Task<Event> CreateAsync(string name, int totalSeats) =>
        events.CreateAsync(name, totalSeats);
}
