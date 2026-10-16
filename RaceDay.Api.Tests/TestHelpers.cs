using System.Net.Http.Json;
using RaceDay.Contracts;

namespace RaceDay.Api.Tests;

/// <summary>Shared setup helpers so each test reads as a short scenario.</summary>
public static class TestHelpers
{
    public const string Password = "P@ssword123";

    public static async Task<(HttpClient Client, AuthResponse User, string Email)> LoginAsync(
        RaceDayApiFactory factory, string role)
    {
        var client = factory.CreateClient();
        var email = $"{role.ToLower()}-{Guid.NewGuid()}@raceday.test";

        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Test",
            LastName = role,
            Email = email,
            Password = Password,
            Role = role
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = Password
        });
        var user = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
        return (client, user, email);
    }

    public static CreateEventRequest NewEvent(string name = "Cape Town 10K") => new()
    {
        Name = name,
        Description = "A scenic city run.",
        EventDate = DateTime.UtcNow.AddMonths(1),
        Location = "Cape Town",
        DistanceKm = 10,
        EventType = "Running"
    };

    public static async Task<EventDto> CreateEventAsync(HttpClient organiser, string name = "Cape Town 10K")
    {
        var response = await organiser.PostAsJsonAsync("/api/events", NewEvent(name));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EventDto>())!;
    }

    public static async Task<CategoryDto> CreateCategoryAsync(HttpClient organiser, int eventId, string name = "Senior")
    {
        var response = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new CreateCategoryRequest { Name = name, DistanceKm = 10m });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    public static async Task<EnrolmentDto> EnrolAsync(HttpClient participant, int eventId, int categoryId)
    {
        var response = await participant.PostAsJsonAsync("/api/enrolments",
            new CreateEnrolmentRequest { EventId = eventId, CategoryId = categoryId });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EnrolmentDto>())!;
    }
}
