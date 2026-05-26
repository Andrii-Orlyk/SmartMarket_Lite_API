using SmartMarket.Application.Common;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Order?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}
