using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
[RequireRole]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ProfileController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMyProfile()
    {
        var userId = HttpContext.GetCurrentUserId();

        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null || user.Role is null)
        {
            return NotFound();
        }

        return Ok(new ProfileDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name
        });
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile(UpdateProfileRequest request)
    {
        var userId = HttpContext.GetCurrentUserId();

        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null || user.Role is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "First name, last name and email are required." });
        }

        var email = request.Email.Trim();
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == email && u.UserId != userId);
        if (emailTaken)
        {
            return Conflict(new { message = "That email is already in use." });
        }

        // A user can only ever update the account tied to their own
        // session UserId - there is no "edit by id" endpoint.
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = email;

        await _db.SaveChangesAsync();

        return Ok(new ProfileDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name
        });
    }
}
