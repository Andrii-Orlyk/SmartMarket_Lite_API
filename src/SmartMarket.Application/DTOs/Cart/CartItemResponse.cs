namespace SmartMarket.Application.DTOs.Cart;

public sealed record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string SKU,
    int Quantity,
    decimal UnitPriceSnapshot,
    decimal LineTotal);
