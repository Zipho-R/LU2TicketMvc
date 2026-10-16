using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private static ResultDto ToDto(Result r) => new()
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        EventId = r.Enrolment?.EventId ?? 0,
        EventName = r.Enrolment?.Event?.Name ?? "",
        FinishTime = r.FinishTime,
        Position = r.Position
    };

    // Organiser records a Participant's finish time and position.
    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<ActionResult<ResultDto>> Create(CreateResultRequest request)
    {
        if (request.Position <= 0)
        {
            return BadRequest(new { message = "Position must be greater than zero." });
        }

        if (request.FinishTime <= TimeSpan.Zero)
        {
            return BadRequest(new { message = "Finish time must be greater than zero." });
        }

        var organiserId = HttpContext.GetCurrentUserId();

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);

        if (enrolment is null || enrolment.Event is null)
        {
            return BadRequest(new { message = "Enrolment does not exist." });
        }

        // Check that the Organiser owns the Event connected to this Enrolment.
        if (enrolment.Event.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        if (enrolment.Result is not null)
        {
            return BadRequest(new { message = "A result has already been captured for this enrolment." });
        }

        var result = new Result
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        result.Enrolment = enrolment;
        return StatusCode(StatusCodes.Status201Created, ToDto(result));
    }

    // Participants may view their own race results only.
    [HttpGet("mine")]
    [RequireRole("Participant")]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetMine()
    {
        var participantId = HttpContext.GetCurrentUserId();

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Where(r => r.Enrolment!.ParticipantId == participantId)
            .ToListAsync();

        return Ok(results.Select(ToDto));
    }

    // Organiser views all results for an Event they own.
    [HttpGet("event/{eventId}")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetForEvent(int eventId)
    {
        var organiserId = HttpContext.GetCurrentUserId();
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();

        if (ev.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Where(r => r.Enrolment!.EventId == eventId)
            .ToListAsync();

        return Ok(results.Select(ToDto));
    }
}
