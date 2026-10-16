using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;
using static RaceDay.Api.Tests.TestHelpers;

namespace RaceDay.Api.Tests;

public class EventTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public EventTests(RaceDayApiFactory factory) => _factory = factory;

    private static UpdateEventRequest UpdateRequest(EventDto ev, string name) => new()
    {
        Name = name,
        Description = ev.Description,
        EventDate = ev.EventDate,
        Location = ev.Location,
        DistanceKm = ev.DistanceKm,
        EventType = ev.EventType
    };

    [Fact]
    public async Task Events_CanBeViewedByBothRoles()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");

        var list = await participant.GetAsync("/api/events");
        var single = await participant.GetAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, single.StatusCode);
    }

    [Fact]
    public async Task GetEvent_ThatDoesNotExist_ReturnsNotFound()
    {
        var (client, _, _) = await LoginAsync(_factory, "Participant");

        var response = await client.GetAsync("/api/events/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidData_ReturnsBadRequest()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var request = NewEvent();
        request.Name = "";
        request.DistanceKm = 0;

        var response = await organiser.PostAsJsonAsync("/api/events", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/events", NewEvent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Mine_ReturnsOnlyOwnEvents()
    {
        var (org1, user1, _) = await LoginAsync(_factory, "Organiser");
        var (org2, _, _) = await LoginAsync(_factory, "Organiser");
        await CreateEventAsync(org1, "Mine");
        await CreateEventAsync(org2, "Not mine");

        var events = await org1.GetFromJsonAsync<List<EventDto>>("/api/events/mine");

        Assert.Single(events!);
        Assert.All(events!, e => Assert.Equal(user1.UserId, e.OrganiserId));
    }

    [Fact]
    public async Task Organiser_CanUpdateOwnEvent()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);

        var response = await organiser.PutAsJsonAsync($"/api/events/{ev.EventId}", UpdateRequest(ev, "Renamed"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Renamed", (await response.Content.ReadFromJsonAsync<EventDto>())!.Name);
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent()
    {
        var (owner, _, _) = await LoginAsync(_factory, "Organiser");
        var (other, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(owner);

        var response = await other.PutAsJsonAsync($"/api/events/{ev.EventId}", UpdateRequest(ev, "Hijacked"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotDeleteAnotherOrganisersEvent()
    {
        var (owner, _, _) = await LoginAsync(_factory, "Organiser");
        var (other, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(owner);

        var response = await other.DeleteAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanDeleteOwnEvent()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);

        var response = await organiser.DeleteAsync($"/api/events/{ev.EventId}");
        var after = await organiser.GetAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotUpdateOrDeleteEvents()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");

        var put = await participant.PutAsJsonAsync($"/api/events/{ev.EventId}", UpdateRequest(ev, "x"));
        var delete = await participant.DeleteAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotDeleteEventThatHasEnrolments()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var cat = await CreateCategoryAsync(organiser, ev.EventId);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");
        await EnrolAsync(participant, ev.EventId, cat.CategoryId);

        var response = await organiser.DeleteAsync($"/api/events/{ev.EventId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
