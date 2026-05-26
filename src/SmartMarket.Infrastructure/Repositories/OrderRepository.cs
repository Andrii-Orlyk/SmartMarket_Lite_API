using Microsoft.EntityFrameworkCore;
using SmartMarket.Application.Common;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Domain.Entities;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.Infrastructure.Repositories;

public sealed class OrderRepository(SmartMarketDbContext context) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Order?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Order?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<Order>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var orders = context.Orders.AsNoTracking().AsQueryable();
        var totalCount = await orders.CountAsync(cancellationToken);

        var items = await orders
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await context.Orders.AddAsync(order, cancellationToken);

    public Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default) =>
        context.Orders
            .AsNoTracking()
            .AnyAsync(x => x.OrderNumber == orderNumber, cancellationToken);
}
