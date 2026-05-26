using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Orders;

namespace SmartMarket.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<IReadOnlyList<OrderResponse>>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<OrderResponse>> GetUserOrderByIdAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<OrderResponse>>> GetAllOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Result<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}
