using System.Net;
using SmartMarket.Application.Common;
using SmartMarket.IntegrationTests.Infrastructure;

namespace SmartMarket.IntegrationTests.Auth;

public sealed class AuthIntegrationTests(SmartMarketWebApplicationFactory factory) : IClassFixture<SmartMarketWebApplicationFactory>
{
    private readonly ApiTestClient _api = new(factory.CreateClient());

    [Fact]
    public async Task Register_ValidUser_ReturnsOkWithToken()
    {
        var email = CreateUniqueEmail();

        var response = await _api.RegisterAsync(email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await ApiTestClient.ReadAuthResponseAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = CreateUniqueEmail();
        await _api.RegisterAsync(email);

        var response = await _api.RegisterAsync(email);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.AuthEmailExists, error.Code);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var email = CreateUniqueEmail();
        await _api.RegisterAsync(email);

        var response = await _api.LoginAsync(email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await ApiTestClient.ReadAuthResponseAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var email = CreateUniqueEmail();
        await _api.RegisterAsync(email);

        var response = await _api.LoginAsync(email, "WrongPassword123!");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.AuthInvalidCredentials, error.Code);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        _api.ClearAuthorization();

        var response = await _api.GetMeAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.AuthUnauthorized, error.Code);
    }

    [Fact]
    public async Task GetCart_WithoutToken_ReturnsUnauthorized()
    {
        var client = new ApiTestClient(factory.CreateClient());

        var response = await client.GetCartAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.AuthUnauthorized, error.Code);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsOk()
    {
        var email = CreateUniqueEmail();
        var registerResponse = await _api.RegisterAsync(email);
        var auth = await ApiTestClient.ReadAuthResponseAsync(registerResponse);

        _api.SetBearerToken(auth.Token);

        var response = await _api.GetMeAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string CreateUniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
}
