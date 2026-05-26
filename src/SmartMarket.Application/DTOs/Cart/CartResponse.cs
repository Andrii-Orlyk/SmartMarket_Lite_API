namespace SmartMarket.Application.DTOs.Cart;

public sealed record CartResponse(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemResponse> Items,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
