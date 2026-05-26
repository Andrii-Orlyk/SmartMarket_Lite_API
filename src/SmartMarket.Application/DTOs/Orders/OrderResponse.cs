using SmartMarket.Domain.Enums;

namespace SmartMarket.Application.DTOs.Orders;

public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemResponse> Items);
