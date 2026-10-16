namespace RaceDay.Api.Models;

public class Role
{
    public int RoleId { get; set; }

    // "Organiser" or "Participant"
    public string Name { get; set; } = "";

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
