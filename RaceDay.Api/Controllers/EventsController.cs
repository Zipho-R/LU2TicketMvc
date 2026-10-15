using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private static EventDto ToDto(Event e) => new()
    {
        EventId = e.EventId,
        Name = e.Name,
        Description = e.Description,
        EventDate = e.EventDate,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        OrganiserId = e.OrganiserId
    };

    // Both roles may view Events - no [RequireRole] here.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var events = await _db.Events.ToListAsync();
        return Ok(events.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        return Ok(ToDto(ev));
    }

    // Events belonging to the logged-in Organiser.
    [HttpGet("mine")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetMine()
    {
        var organiserId = HttpContext.GetCurrentUserId();
        var events = await _db.Events.Where(e => e.OrganiserId == organiserId).ToListAsync();
        return Ok(events.Select(ToDto));
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<ActionResult<EventDto>> Create(CreateEventRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId();

        var ev = new Event
        {
            Name = request.Name,
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location,
            DistanceKm = request.DistanceKm,
            EventType = request.EventType,
            OrganiserId = organiserId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ToDto(ev));
    }

    [HttpPut("{id}")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<EventDto>> Update(int id, UpdateEventRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId();
        var ev = await _db.Events.FindAsync(id);

        if (ev is null) return NotFound();

        // An Organiser may only update Events they own.
        if (ev.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        ev.Name = request.Name;
        ev.Description = request.Description;
        ev.EventDate = request.EventDate;
        ev.Location = request.Location;
        ev.DistanceKm = request.DistanceKm;
        ev.EventType = request.EventType;

        await _db.SaveChangesAsync();
        return Ok(ToDto(ev));
    }

    [HttpDelete("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var organiserId = HttpContext.GetCurrentUserId();
        var ev = await _db.Events.FindAsync(id);

        if (ev is null) return NotFound();

        if (ev.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
