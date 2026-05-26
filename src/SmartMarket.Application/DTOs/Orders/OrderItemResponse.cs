namespace SmartMarket.Application.DTOs.Orders;

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductNameSnapshot,
    decimal UnitPriceSnapshot,
    int Quantity,
    decimal LineTotal);
