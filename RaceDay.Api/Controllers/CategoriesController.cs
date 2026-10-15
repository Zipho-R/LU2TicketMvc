using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        CategoryId = c.CategoryId,
        Name = c.Name,
        EventId = c.EventId
    };

    // Both roles may view categories for an Event.
    [HttpGet("api/events/{eventId}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetForEvent(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists) return NotFound();

        var categories = await _db.Categories.Where(c => c.EventId == eventId).ToListAsync();
        return Ok(categories.Select(ToDto));
    }

    [HttpPost("api/events/{eventId}/categories")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<CategoryDto>> Create(int eventId, CreateCategoryRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId();
        var ev = await _db.Events.FindAsync(eventId);

        if (ev is null) return NotFound();

        // Only the Organiser who owns the Event may add categories to it.
        if (ev.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        var category = new Category { Name = request.Name, EventId = eventId };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForEvent), new { eventId }, ToDto(category));
    }

    [HttpPut("api/categories/{id}")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CreateCategoryRequest request)
    {
        var organiserId = HttpContext.GetCurrentUserId();

        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null || category.Event is null) return NotFound();

        if (category.Event.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        category.Name = request.Name;
        await _db.SaveChangesAsync();

        return Ok(ToDto(category));
    }

    [HttpDelete("api/categories/{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var organiserId = HttpContext.GetCurrentUserId();

        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null || category.Event is null) return NotFound();

        if (category.Event.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this Event." });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
