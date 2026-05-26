using Microsoft.EntityFrameworkCore;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Domain.Entities;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.Infrastructure.Repositories;

public sealed class UserRepository(SmartMarketDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await context.Users.AddAsync(user, cancellationToken);
}
