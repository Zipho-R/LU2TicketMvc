namespace RaceDay.Contracts;

// ---------- Auth ----------

public class RegisterRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

    // Expected values: "Organiser" or "Participant"
    public string Role { get; set; } = "";
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

// ---------- Profile ----------

public class ProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
}

// ---------- Events ----------

public class CreateEventRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public double DistanceKm { get; set; }
    public string EventType { get; set; } = "";
}

public class UpdateEventRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public double DistanceKm { get; set; }
    public string EventType { get; set; } = "";
}

public class EventDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public double DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public int OrganiserId { get; set; }
}

// ---------- Categories ----------

public class CreateCategoryRequest
{
    public string Name { get; set; } = "";
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public int EventId { get; set; }
}

// ---------- Enrolments ----------

public class CreateEnrolmentRequest
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateEnrolmentStatusRequest
{
    // Expected values: "Confirmed", "Cancelled"
    public string Status { get; set; } = "";
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string Status { get; set; } = "";
}

// ---------- Results ----------

public class CreateResultRequest
{
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}

// ---------- Part 3 (placeholder for image uploads) ----------

public class UploadResponse
{
    public string Url { get; set; } = "";
}
