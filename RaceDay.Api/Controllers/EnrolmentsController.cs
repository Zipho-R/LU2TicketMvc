using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private static EnrolmentDto ToDto(Enrolment en) => new()
    {
        EnrolmentId = en.EnrolmentId,
        ParticipantId = en.ParticipantId,
        ParticipantName = en.Participant is null ? "" : $"{en.Participant.FirstName} {en.Participant.LastName}",
        EventId = en.EventId,
        CategoryId = en.CategoryId,
        CategoryName = en.Category?.Name ?? "",
        Status = en.Status
    };

    // A Participant enters an Event by choosing the Event and a valid Category.
    [HttpPost]
    [RequireRole("Participant")]
    public async Task<ActionResult<EnrolmentDto>> Create(CreateEnrolmentRequest request)
    {
        var participantId = HttpContext.GetCurrentUserId();

        var ev = await _db.Events.FindAsync(request.EventId);
        if (ev is null) return BadRequest(new { message = "Event does not exist." });

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.EventId == request.EventId);
        if (category is null) return BadRequest(new { message = "Category is not valid for this Event." });

        var alreadyEnrolled = await _db.Enrolments.AnyAsync(e =>
            e.ParticipantId == participantId && e.EventId == request.EventId);
        if (alreadyEnrolled) return Conflict(new { message = "You are already enrolled in this Event." });

        var enrolment = new Enrolment
        {
            ParticipantId = participantId,
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            Status = "Confirmed"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        await _db.Entry(enrolment).Reference(e => e.Participant).LoadAsync();
        await _db.Entry(enrolment).Reference(e => e.Category).LoadAsync();

        return CreatedAtAction(nameof(GetMine), null, ToDto(enrolment));
    }

    // Enrolments belonging to the logged-in Participant.
    [HttpGet("mine")]
    [RequireRole("Participant")]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetMine()
    {
        var participantId = HttpContext.GetCurrentUserId();

        var enrolments = await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Where(e => e.ParticipantId == participantId)
            .ToListAsync();

        return Ok(enrolments.Select(ToDto));
    }

    // Enrolments for a specific Event - Organiser who owns the Event only.
    [HttpGet("event/{eventId}")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetForEvent(int eventId)
    {
        var organiserId = HttpContext.GetCurrentUserId();
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();

        if (ev.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        var enrolments = await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Where(e => e.EventId == eventId)
            .ToListAsync();

        return Ok(enrolments.Select(ToDto));
    }

    [HttpPut("{id}/status")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<EnrolmentDto>> UpdateStatus(int id, UpdateEnrolmentStatusRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId();

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment is null || enrolment.Event is null) return NotFound();

        // Only the correct Organiser can manage enrolments for their own Events.
        if (enrolment.Event.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        var validStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
        if (!validStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Status must be Pending, Confirmed or Cancelled." });
        }

        enrolment.Status = request.Status;
        await _db.SaveChangesAsync();

        return Ok(ToDto(enrolment));
    }
}
