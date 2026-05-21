using Microsoft.AspNetCore.Mvc;
using TicketingService.Application;

namespace TicketingService.Controllers;

[ApiController]
[Route("events/{eventId:int}/bookings")]
public class BookingsController(BookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByEvent(int eventId) =>
        Ok(await bookings.GetByEventIdAsync(eventId));
}
