using Microsoft.AspNetCore.Mvc;
using TicketingService.Application;
using TicketingService.Application.Strategies;

namespace TicketingService.Controllers;

[ApiController]
[Route("events/{eventId:int}/bookings")]
public class BookingsController(BookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByEvent(int eventId) =>
        Ok(await bookings.GetByEventIdAsync(eventId));

    [HttpPost]
    public async Task<IActionResult> Book(
        int eventId,
        [FromQuery] string strategy,
        [FromBody] CreateBookingRequest request)
    {
        try
        {
            var booking = await bookings.BookAsync(eventId, request.UserId, strategy);
            return CreatedAtAction(nameof(GetByEvent), new { eventId }, booking);
        }
        catch (NoSeatsAvailableException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}

public record CreateBookingRequest(int UserId);
