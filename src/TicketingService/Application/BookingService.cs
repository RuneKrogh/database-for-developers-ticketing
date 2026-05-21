using TicketingService.Application.Strategies;
using TicketingService.Domain;
using TicketingService.Infrastructure.Repositories;

namespace TicketingService.Application;

public class BookingService(BookingRepository bookings, IServiceProvider serviceProvider)
{
    public Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId) =>
        bookings.GetByEventIdAsync(eventId);

    public Task<Booking> BookAsync(int eventId, int userId, string strategy)
    {
        var bookingStrategy = serviceProvider.GetRequiredKeyedService<IBookingStrategy>(strategy);
        return bookingStrategy.BookAsync(eventId, userId);
    }
}
