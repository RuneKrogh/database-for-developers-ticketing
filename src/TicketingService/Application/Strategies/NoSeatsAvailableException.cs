namespace TicketingService.Application.Strategies;

public class NoSeatsAvailableException(int eventId)
    : Exception($"No seats available for event {eventId}.");
