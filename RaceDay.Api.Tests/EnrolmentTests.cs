using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;
using static RaceDay.Api.Tests.TestHelpers;

namespace RaceDay.Api.Tests;

public class EnrolmentTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public EnrolmentTests(RaceDayApiFactory factory) => _factory = factory;

    private async Task<(HttpClient Organiser, HttpClient Participant, EventDto Event, CategoryDto Category)> SetupAsync()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var cat = await CreateCategoryAsync(organiser, ev.EventId);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");
        return (organiser, participant, ev, cat);
    }

    [Fact]
    public async Task Organiser_CannotEnrol()
    {
        var (organiser, _, ev, cat) = await SetupAsync();

        var response = await organiser.PostAsJsonAsync("/api/enrolments",
            new CreateEnrolmentRequest { EventId = ev.EventId, CategoryId = cat.CategoryId });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/enrolments",
            new CreateEnrolmentRequest { EventId = 1, CategoryId = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_InMissingEvent_ReturnsBadRequest()
    {
        var (participant, _, _) = await LoginAsync(_factory, "Participant");

        var response = await participant.PostAsJsonAsync("/api/enrolments",
            new CreateEnrolmentRequest { EventId = 999999, CategoryId = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryFromAnotherEvent_ReturnsBadRequest()
    {
        var (organiser, participant, eventA, _) = await SetupAsync();
        var eventB = await CreateEventAsync(organiser, "Event B");
        var catB = await CreateCategoryAsync(organiser, eventB.EventId);

        var response = await participant.PostAsJsonAsync("/api/enrolments",
            new CreateEnrolmentRequest { EventId = eventA.EventId, CategoryId = catB.CategoryId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_TwiceInSameEvent_ReturnsConflict()
    {
        var (organiser, participant, ev, cat1) = await SetupAsync();
        var cat2 = await CreateCategoryAsync(organiser, ev.EventId, "Masters");
        await EnrolAsync(participant, ev.EventId, cat1.CategoryId);

        var again = await participant.PostAsJsonAsync("/api/enrolments",
            new CreateEnrolmentRequest { EventId = ev.EventId, CategoryId = cat2.CategoryId });

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task Participant_SeesOnlyOwnEnrolments()
    {
        var (_, p1, ev, cat) = await SetupAsync();
        var (p2, _, _) = await LoginAsync(_factory, "Participant");
        var first = await EnrolAsync(p1, ev.EventId, cat.CategoryId);
        await EnrolAsync(p2, ev.EventId, cat.CategoryId);

        var mine = await p1.GetFromJsonAsync<List<EnrolmentDto>>("/api/enrolments/mine");

        Assert.Single(mine!);
        Assert.Equal(first.EnrolmentId, mine![0].EnrolmentId);
    }

    [Fact]
    public async Task Organiser_CanViewEnrolmentsForOwnEvent()
    {
        var (organiser, participant, ev, cat) = await SetupAsync();
        await EnrolAsync(participant, ev.EventId, cat.CategoryId);

        var response = await organiser.GetAsync($"/api/enrolments/event/{ev.EventId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single((await response.Content.ReadFromJsonAsync<List<EnrolmentDto>>())!);
    }

    [Fact]
    public async Task Organiser_CannotViewEnrolmentsForAnotherOrganisersEvent()
    {
        var (_, _, ev, _) = await SetupAsync();
        var (other, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await other.GetAsync($"/api/enrolments/event/{ev.EventId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanUpdateEnrolmentStatus_OnOwnEvent()
    {
        var (organiser, participant, ev, cat) = await SetupAsync();
        var enrolment = await EnrolAsync(participant, ev.EventId, cat.CategoryId);

        var response = await organiser.PutAsJsonAsync($"/api/enrolments/{enrolment.EnrolmentId}/status",
            new UpdateEnrolmentStatusRequest { Status = "Cancelled" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Cancelled", (await response.Content.ReadFromJsonAsync<EnrolmentDto>())!.Status);
    }

    [Fact]
    public async Task UpdateEnrolmentStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var (organiser, participant, ev, cat) = await SetupAsync();
        var enrolment = await EnrolAsync(participant, ev.EventId, cat.CategoryId);

        var response = await organiser.PutAsJsonAsync($"/api/enrolments/{enrolment.EnrolmentId}/status",
            new UpdateEnrolmentStatusRequest { Status = "Banana" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUpdateStatus_OnAnotherOrganisersEvent()
    {
        var (_, participant, ev, cat) = await SetupAsync();
        var enrolment = await EnrolAsync(participant, ev.EventId, cat.CategoryId);
        var (other, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await other.PutAsJsonAsync($"/api/enrolments/{enrolment.EnrolmentId}/status",
            new UpdateEnrolmentStatusRequest { Status = "Cancelled" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotUpdateEnrolmentStatus()
    {
        var (_, participant, ev, cat) = await SetupAsync();
        var enrolment = await EnrolAsync(participant, ev.EventId, cat.CategoryId);

        var response = await participant.PutAsJsonAsync($"/api/enrolments/{enrolment.EnrolmentId}/status",
            new UpdateEnrolmentStatusRequest { Status = "Cancelled" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
