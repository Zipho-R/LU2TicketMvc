namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryId { get; set; }

    // e.g. "Under 20", "Senior", "10km", "21km"
    public string Name { get; set; } = "";

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
