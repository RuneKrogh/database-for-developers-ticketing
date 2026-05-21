namespace TicketingService.Domain;

public class Booking
{
    public int Id { get; init; }
    public int EventId { get; init; }
    public int UserId { get; init; }
    public DateTime BookedAt { get; init; }
}
