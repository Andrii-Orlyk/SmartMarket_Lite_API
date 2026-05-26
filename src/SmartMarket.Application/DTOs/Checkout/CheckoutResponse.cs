using SmartMarket.Application.DTOs.Orders;

namespace SmartMarket.Application.DTOs.Checkout;

public sealed record CheckoutResponse(OrderResponse Order);
