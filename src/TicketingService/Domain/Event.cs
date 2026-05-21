namespace TicketingService.Domain;

public class Event
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public int TotalSeats { get; init; }
    public int AvailableSeats { get; init; }
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
}
