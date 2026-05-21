namespace TicketingService.Application.Strategies;

public interface IBookingStrategy
{
    Task<Domain.Booking> BookAsync(int eventId, int userId);
}
