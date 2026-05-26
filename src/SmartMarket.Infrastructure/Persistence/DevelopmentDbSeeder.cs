using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Application.Interfaces.Security;
using SmartMarket.Application.Options;
using SmartMarket.Domain.Entities;
using SmartMarket.Domain.Enums;

namespace SmartMarket.Infrastructure.Persistence;

public static class DevelopmentDbSeeder
{
    public static async Task SeedAsync(
        SmartMarketDbContext context,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IOptions<SeedAdminSettings> seedOptions,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        var seed = seedOptions.Value;
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
        {
            return;
        }

        var email = seed.Email.Trim().ToLowerInvariant();
        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(seed.Password),
            FirstName = seed.FirstName,
            LastName = seed.LastName,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(admin, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Development admin user seeded for {Email}", email);
    }
}
