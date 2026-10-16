using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task Register_WithInvalidRole_Fails()
    {
        var request = NewParticipant("bad-role@raceday.test");
        request.Role = "Admin";

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingPassword_Fails()
    {
        var request = NewParticipant("no-password@raceday.test");
        request.Password = "";

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotPlainText()
    {
        var request = NewParticipant("hash-check@raceday.test");

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(request.Password, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDay.Api.Data.RaceDayDbContext>();
        var user = db.Users.Single(u => u.Email == request.Email);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
        Assert.NotEqual(request.Password, user.PasswordHash);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccessProtectedProfile()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
