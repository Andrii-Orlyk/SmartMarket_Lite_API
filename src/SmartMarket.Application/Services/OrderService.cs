using FluentValidation;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Orders;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Application.Interfaces.Services;
using SmartMarket.Domain.Entities;
using SmartMarket.Domain.Enums;

namespace SmartMarket.Application.Services;

public sealed class OrderService(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateOrderStatusRequest> updateStatusValidator) : IOrderService
{
    private static readonly IReadOnlyDictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions =
        new Dictionary<OrderStatus, HashSet<OrderStatus>>
        {
            [OrderStatus.Pending] = [OrderStatus.Paid, OrderStatus.Cancelled],
            [OrderStatus.Paid] = [OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.Completed] = [],
            [OrderStatus.Cancelled] = []
        };

    public async Task<Result<IReadOnlyList<OrderResponse>>> GetUserOrdersAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetByUserIdAsync(userId, cancellationToken);
        return Result<IReadOnlyList<OrderResponse>>.Success(orders.Select(MapOrder).ToList());
    }

    public async Task<Result<OrderResponse>> GetUserOrderByIdAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdForUserAsync(orderId, userId, cancellationToken);
        if (order is null)
        {
            return Result<OrderResponse>.Failure(ErrorCodes.OrderNotFound, "Order not found.");
        }

        return Result<OrderResponse>.Success(MapOrder(order));
    }

    public async Task<Result<PagedResult<OrderResponse>>> GetAllOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetAllPagedAsync(page, pageSize, cancellationToken);
        return Result<PagedResult<OrderResponse>>.Success(new PagedResult<OrderResponse>
        {
            Items = orders.Items.Select(MapOrder).ToList(),
            Page = orders.Page,
            PageSize = orders.PageSize,
            TotalCount = orders.TotalCount
        });
    }

    public async Task<Result<OrderResponse>> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<OrderResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var order = await orderRepository.GetTrackedByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderResponse>.Failure(ErrorCodes.OrderNotFound, "Order not found.");
        }

        if (!IsValidTransition(order.Status, request.Status))
        {
            return Result<OrderResponse>.Failure(
                ErrorCodes.OrderInvalidStatusTransition,
                "Order status transition is not allowed.");
        }

        order.Status = request.Status;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedOrder = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        return Result<OrderResponse>.Success(MapOrder(updatedOrder!));
    }

    public static OrderResponse MapOrder(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.Items
                .Select(item => new OrderItemResponse(
                    item.Id,
                    item.ProductId,
                    item.ProductNameSnapshot,
                    item.UnitPriceSnapshot,
                    item.Quantity,
                    item.LineTotal))
                .ToList());

    private static bool IsValidTransition(OrderStatus currentStatus, OrderStatus newStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus);
}
