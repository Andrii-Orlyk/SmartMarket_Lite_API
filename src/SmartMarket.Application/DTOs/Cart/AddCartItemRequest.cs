namespace SmartMarket.Application.DTOs.Cart;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
