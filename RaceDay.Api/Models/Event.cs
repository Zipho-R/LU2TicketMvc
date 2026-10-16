namespace RaceDay.Api.Models;

public class Event
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public double DistanceKm { get; set; }

    // e.g. "Running", "Cycling", "Walking"
    public string EventType { get; set; } = "";

    public int OrganiserId { get; set; }
    public AppUser? Organiser { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
