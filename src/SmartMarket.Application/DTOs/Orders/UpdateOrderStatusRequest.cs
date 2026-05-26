using SmartMarket.Domain.Enums;

namespace SmartMarket.Application.DTOs.Orders;

public sealed record UpdateOrderStatusRequest(OrderStatus Status);
