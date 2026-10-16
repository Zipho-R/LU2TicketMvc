using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;
using static RaceDay.Api.Tests.TestHelpers;

namespace RaceDay.Api.Tests;

public class ResultTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public ResultTests(RaceDayApiFactory factory) => _factory = factory;

    private async Task<(HttpClient Organiser, HttpClient Participant, EventDto Event, EnrolmentDto Enrolment)> SetupAsync()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var cat = await CreateCategoryAsync(organiser, ev.EventId);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");
        var enrolment = await EnrolAsync(participant, ev.EventId, cat.CategoryId);
        return (organiser, participant, ev, enrolment);
    }

    private static CreateResultRequest NewResult(int enrolmentId) => new()
    {
        EnrolmentId = enrolmentId,
        FinishTime = TimeSpan.FromMinutes(52),
        Position = 3
    };

    [Fact]
    public async Task Organiser_CanCaptureResult_ForOwnEvent()
    {
        var (organiser, _, _, enrolment) = await SetupAsync();

        var response = await organiser.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(3, (await response.Content.ReadFromJsonAsync<ResultDto>())!.Position);
    }

    [Fact]
    public async Task Participant_CannotCaptureResult()
    {
        var (_, participant, _, enrolment) = await SetupAsync();

        var response = await participant.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotCaptureResult_ForAnotherOrganisersEvent()
    {
        var (_, _, _, enrolment) = await SetupAsync();
        var (other, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await other.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_Twice_ReturnsBadRequest()
    {
        var (organiser, _, _, enrolment) = await SetupAsync();
        await organiser.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));

        var again = await organiser.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));

        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_ForMissingEnrolment_ReturnsBadRequest()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await organiser.PostAsJsonAsync("/api/results", NewResult(999999));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_WithInvalidPosition_ReturnsBadRequest()
    {
        var (organiser, _, _, enrolment) = await SetupAsync();
        var request = NewResult(enrolment.EnrolmentId);
        request.Position = 0;

        var response = await organiser.PostAsJsonAsync("/api/results", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CanViewOwnResults()
    {
        var (organiser, participant, _, enrolment) = await SetupAsync();
        await organiser.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));

        var response = await participant.GetAsync("/api/results/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single((await response.Content.ReadFromJsonAsync<List<ResultDto>>())!);
    }

    [Fact]
    public async Task Participant_DoesNotSeeOtherParticipantsResults()
    {
        var (organiser, _, _, enrolment) = await SetupAsync();
        await organiser.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));
        var (stranger, _, _) = await LoginAsync(_factory, "Participant");

        var results = await stranger.GetFromJsonAsync<List<ResultDto>>("/api/results/mine");

        Assert.Empty(results!);
    }

    [Fact]
    public async Task Organiser_CanViewResultsForOwnEvent_ButNotForOthers()
    {
        var (organiser, _, ev, enrolment) = await SetupAsync();
        await organiser.PostAsJsonAsync("/api/results", NewResult(enrolment.EnrolmentId));
        var (other, _, _) = await LoginAsync(_factory, "Organiser");

        var own = await organiser.GetAsync($"/api/results/event/{ev.EventId}");
        var foreign = await other.GetAsync($"/api/results/event/{ev.EventId}");

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Single((await own.Content.ReadFromJsonAsync<List<ResultDto>>())!);
        Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUseParticipantOnlyResultsEndpoint()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await organiser.GetAsync("/api/results/mine");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
