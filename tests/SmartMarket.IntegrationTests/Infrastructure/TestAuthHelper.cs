using Microsoft.Extensions.DependencyInjection;
using SmartMarket.Application.Interfaces.Security;
using SmartMarket.Domain.Entities;
using SmartMarket.Domain.Enums;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.IntegrationTests.Infrastructure;

public static class TestAuthHelper
{
    public static async Task<ApiTestClient> CreateAdminClientAsync(SmartMarketWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartMarketDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var email = $"admin-{Guid.NewGuid():N}@test.local";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash("Password123!"),
            FirstName = "Admin",
            LastName = "Test",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var api = new ApiTestClient(factory.CreateClient());
        api.SetBearerToken(jwtTokenService.GenerateToken(user));
        return api;
    }

    public static async Task<ApiTestClient> CreateUserClientAsync(SmartMarketWebApplicationFactory factory)
    {
        var api = new ApiTestClient(factory.CreateClient());
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var response = await api.RegisterAsync(email);
        response.EnsureSuccessStatusCode();

        var auth = await ApiTestClient.ReadAuthResponseAsync(response);
        api.SetBearerToken(auth.Token);
        return api;
    }
}
