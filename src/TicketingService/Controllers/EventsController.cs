using Microsoft.AspNetCore.Mvc;
using TicketingService.Application;

namespace TicketingService.Controllers;

[ApiController]
[Route("events")]
public class EventsController(EventService events) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await events.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await events.GetByIdAsync(id);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var ev = await events.CreateAsync(request.Name, request.TotalSeats);
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ev);
    }
}

public record CreateEventRequest(string Name, int TotalSeats);
