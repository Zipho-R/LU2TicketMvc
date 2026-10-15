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
        EventId = c.EventId,
        DistanceKm = c.DistanceKm
    };

    // Both roles may view categories for an event.
    [HttpGet("api/events/{eventId}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetForEvent(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
            return NotFound();

        var categories = await _db.Categories
            .Where(c => c.EventId == eventId)
            .ToListAsync();

        return Ok(categories.Select(ToDto));
    }

    // Only the organiser who owns the event may create categories.
    [HttpPost("api/events/{eventId}/categories")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<CategoryDto>> Create(
        int eventId,
        CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (request.DistanceKm <= 0 || request.DistanceKm > 999.99m)
        {
            return BadRequest(new
            {
                message = "Category distance must be greater than zero and no more than 999.99 km."
            });
        }

        var organiserId = HttpContext.GetCurrentUserId();
        var ev = await _db.Events.FindAsync(eventId);

        if (ev is null)
            return NotFound();

        if (ev.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You do not own this event."
            });
        }

        var categoryName = request.Name.Trim();

        var duplicateName = await _db.Categories.AnyAsync(c =>
            c.EventId == eventId &&
            c.Name == categoryName);

        if (duplicateName)
        {
            return Conflict(new
            {
                message = "This category already exists for the event."
            });
        }

        var category = new Category
        {
            Name = categoryName,
            DistanceKm = request.DistanceKm,
            EventId = eventId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetForEvent),
            new { eventId },
            ToDto(category));
    }

    // Only the organiser who owns the event may update its categories.
    [HttpPut("api/categories/{id}")]
    [RequireRole("Organiser")]
    public async Task<ActionResult<CategoryDto>> Update(
        int id,
        CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (request.DistanceKm <= 0 || request.DistanceKm > 999.99m)
        {
            return BadRequest(new
            {
                message = "Category distance must be greater than zero and no more than 999.99 km."
            });
        }

        var organiserId = HttpContext.GetCurrentUserId();

        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null || category.Event is null)
            return NotFound();

        if (category.Event.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You do not own this event."
            });
        }

        var categoryName = request.Name.Trim();

        var duplicateName = await _db.Categories.AnyAsync(c =>
            c.EventId == category.EventId &&
            c.CategoryId != id &&
            c.Name == categoryName);

        if (duplicateName)
        {
            return Conflict(new
            {
                message = "This category already exists for the event."
            });
        }

        category.Name = categoryName;
        category.DistanceKm = request.DistanceKm;

        await _db.SaveChangesAsync();

        return Ok(ToDto(category));
    }

    // Only the organiser who owns the event may delete its categories.
    [HttpDelete("api/categories/{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var organiserId = HttpContext.GetCurrentUserId();

        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null || category.Event is null)
            return NotFound();

        if (category.Event.OrganiserId != organiserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You do not own this event."
            });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}