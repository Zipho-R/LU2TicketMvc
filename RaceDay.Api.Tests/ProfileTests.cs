using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;
using static RaceDay.Api.Tests.TestHelpers;

namespace RaceDay.Api.Tests;

public class ProfileTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public ProfileTests(RaceDayApiFactory factory) => _factory = factory;

    [Theory]
    [InlineData("Organiser")]
    [InlineData("Participant")]
    public async Task BothRoles_CanViewOwnProfile(string role)
    {
        var (client, user, email) = await LoginAsync(_factory, role);

        var response = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal(user.UserId, profile!.UserId);
        Assert.Equal(email, profile.Email);
        Assert.Equal(role, profile.Role);
    }

    [Theory]
    [InlineData("Organiser")]
    [InlineData("Participant")]
    public async Task BothRoles_CanUpdateOwnProfile(string role)
    {
        var (client, _, email) = await LoginAsync(_factory, role);

        var response = await client.PutAsJsonAsync("/api/profile/me", new UpdateProfileRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = email
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("Updated", profile!.FirstName);
    }

    [Fact]
    public async Task UpdateProfile_WithEmailOfAnotherUser_ReturnsConflict()
    {
        var (_, _, otherEmail) = await LoginAsync(_factory, "Participant");
        var (client, _, _) = await LoginAsync(_factory, "Participant");

        var response = await client.PutAsJsonAsync("/api/profile/me", new UpdateProfileRequest
        {
            FirstName = "A",
            LastName = "B",
            Email = otherEmail
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithMissingFields_ReturnsBadRequest()
    {
        var (client, _, _) = await LoginAsync(_factory, "Participant");

        var response = await client.PutAsJsonAsync("/api/profile/me", new UpdateProfileRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PutAsJsonAsync("/api/profile/me", new UpdateProfileRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        var (client, _, _) = await LoginAsync(_factory, "Participant");

        var logout = await client.PostAsync("/api/auth/logout", null);
        var after = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }
}
