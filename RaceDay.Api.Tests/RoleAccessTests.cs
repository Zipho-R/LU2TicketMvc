using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;

namespace RaceDay.Api.Tests;

public class RoleAccessTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public RoleAccessTests(RaceDayApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> LoggedInClientAsync(string role, string emailPrefix)
    {
        var client = _factory.CreateClient();
        var email = $"{emailPrefix}-{Guid.NewGuid()}@raceday.test";

        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Test",
            LastName = role,
            Email = email,
            Password = "P@ssword123",
            Role = role
        });

        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "P@ssword123"
        });

        return client;
    }

    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        var organiser = await LoggedInClientAsync("Organiser", "org-create");

        var response = await organiser.PostAsJsonAsync("/api/events", new CreateEventRequest
        {
            Name = "Cape Town 10K",
            Description = "A scenic city run.",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Cape Town",
            DistanceKm = 10,
            EventType = "Running"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var participant = await LoggedInClientAsync("Participant", "part-create");

        var response = await participant.PostAsJsonAsync("/api/events", new CreateEventRequest
        {
            Name = "Should Not Be Created",
            Description = "n/a",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Nowhere",
            DistanceKm = 5,
            EventType = "Running"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CanEnrolInEvent()
    {
        var organiser = await LoggedInClientAsync("Organiser", "org-enrol");

        var createEventResponse = await organiser.PostAsJsonAsync("/api/events", new CreateEventRequest
        {
            Name = "Durban Half Marathon",
            Description = "Coastal half marathon.",
            EventDate = DateTime.UtcNow.AddMonths(2),
            Location = "Durban",
            DistanceKm = 21.1,
            EventType = "Running"
        });
        var createdEvent = await createEventResponse.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(createdEvent);

        var categoryResponse = await organiser.PostAsJsonAsync(
            $"/api/events/{createdEvent!.EventId}/categories",
            new CreateCategoryRequest { Name = "Senior" });
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(createdCategory);

        var participant = await LoggedInClientAsync("Participant", "part-enrol");

        var enrolResponse = await participant.PostAsJsonAsync("/api/enrolments", new CreateEnrolmentRequest
        {
            EventId = createdEvent.EventId,
            CategoryId = createdCategory!.CategoryId
        });

        Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotAccessOrganiserOnlyEndpoint()
    {
        var organiser = await LoggedInClientAsync("Organiser", "org-guard");
        var createEventResponse = await organiser.PostAsJsonAsync("/api/events", new CreateEventRequest
        {
            Name = "Guarded Event",
            Description = "n/a",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Cape Town",
            DistanceKm = 5,
            EventType = "Running"
        });
        var createdEvent = await createEventResponse.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(createdEvent);

        var participant = await LoggedInClientAsync("Participant", "part-guard");

        // Viewing enrolments for an Event is Organiser-only.
        var response = await participant.GetAsync($"/api/enrolments/event/{createdEvent!.EventId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
