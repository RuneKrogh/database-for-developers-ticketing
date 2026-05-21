using TicketingService.Domain;
using TicketingService.Infrastructure.Repositories;

namespace TicketingService.Application;

public class BookingService(BookingRepository bookings)
{
    public Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId) =>
        bookings.GetByEventIdAsync(eventId);
}
