namespace RaceDay.Api.Models;

public class AppUser
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    // Unique - enforced in RaceDayDbContext.OnModelCreating
    public string Email { get; set; } = "";

    // Never store a plain-text password. Only the generated hash is stored.
    public string PasswordHash { get; set; } = "";

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
