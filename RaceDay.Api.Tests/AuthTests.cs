using System.Net;
using System.Net.Http.Json;
using RaceDay.Contracts;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public AuthTests(RaceDayApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequest NewParticipant(string email) => new()
    {
        FirstName = "Zipho",
        LastName = "Runner",
        Email = email,
        Password = "P@ssword123",
        Role = "Participant"
    };

    [Fact]
    public async Task Register_WithValidData_Succeeds()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", NewParticipant("valid-register@raceday.test"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Fails()
    {
        var client = _factory.CreateClient();
        var request = NewParticipant("duplicate@raceday.test");

        await client.PostAsJsonAsync("/api/auth/register", request);
        var secondResponse = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Succeeds()
    {
        var client = _factory.CreateClient();
        var request = NewParticipant("login-success@raceday.test");
        await client.PostAsJsonAsync("/api/auth/register", request);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal("Participant", body!.Role);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_Fails()
    {
        var client = _factory.CreateClient();
        var request = NewParticipant("login-fail@raceday.test");
        await client.PostAsJsonAsync("/api/auth/register", request);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = request.Email,
            Password = "TheWrongPassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Fails()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "nobody-here@raceday.test",
            Password = "whatever"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccessProtectedProfile()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
