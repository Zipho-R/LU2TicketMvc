using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;
using static RaceDay.Api.Tests.TestHelpers;

namespace RaceDay.Api.Tests;

public class CategoryTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public CategoryTests(RaceDayApiFactory factory) => _factory = factory;

    [Fact]
    public async Task BothRoles_CanViewCategories()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        await CreateCategoryAsync(organiser, ev.EventId);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");

        var response = await participant.GetAsync($"/api/events/{ev.EventId}/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single((await response.Content.ReadFromJsonAsync<List<CategoryDto>>())!);
    }

    [Fact]
    public async Task Participant_CannotCreateCategory()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");

        var response = await participant.PostAsJsonAsync($"/api/events/{ev.EventId}/categories",
            new CreateCategoryRequest { Name = "Sneaky", DistanceKm = 5m });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotCreateCategoryOnAnotherOrganisersEvent()
    {
        var (owner, _, _) = await LoginAsync(_factory, "Organiser");
        var (other, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(owner);

        var response = await other.PostAsJsonAsync($"/api/events/{ev.EventId}/categories",
            new CreateCategoryRequest { Name = "Nope", DistanceKm = 5m });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ForMissingEvent_ReturnsNotFound()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await organiser.PostAsJsonAsync("/api/events/999999/categories",
            new CreateCategoryRequest { Name = "Ghost", DistanceKm = 5m });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithInvalidDistance_ReturnsBadRequest()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);

        var response = await organiser.PostAsJsonAsync($"/api/events/{ev.EventId}/categories",
            new CreateCategoryRequest { Name = "Bad", DistanceKm = 0m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsConflict()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        await CreateCategoryAsync(organiser, ev.EventId, "Senior");

        var response = await organiser.PostAsJsonAsync($"/api/events/{ev.EventId}/categories",
            new CreateCategoryRequest { Name = "Senior", DistanceKm = 10m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanUpdateAndDeleteOwnCategory()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var cat = await CreateCategoryAsync(organiser, ev.EventId);

        var update = await organiser.PutAsJsonAsync($"/api/categories/{cat.CategoryId}",
            new CreateCategoryRequest { Name = "Under 20", DistanceKm = 5m });
        var delete = await organiser.DeleteAsync($"/api/categories/{cat.CategoryId}");

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Under 20", (await update.Content.ReadFromJsonAsync<CategoryDto>())!.Name);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUpdateOrDeleteAnotherOrganisersCategory()
    {
        var (owner, _, _) = await LoginAsync(_factory, "Organiser");
        var (other, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(owner);
        var cat = await CreateCategoryAsync(owner, ev.EventId);

        var update = await other.PutAsJsonAsync($"/api/categories/{cat.CategoryId}",
            new CreateCategoryRequest { Name = "Hijack", DistanceKm = 5m });
        var delete = await other.DeleteAsync($"/api/categories/{cat.CategoryId}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ThatDoesNotExist_ReturnsNotFound()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");

        var response = await organiser.DeleteAsync("/api/categories/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotDeleteCategoryThatHasEnrolments()
    {
        var (organiser, _, _) = await LoginAsync(_factory, "Organiser");
        var ev = await CreateEventAsync(organiser);
        var cat = await CreateCategoryAsync(organiser, ev.EventId);
        var (participant, _, _) = await LoginAsync(_factory, "Participant");
        await EnrolAsync(participant, ev.EventId, cat.CategoryId);

        var response = await organiser.DeleteAsync($"/api/categories/{cat.CategoryId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
