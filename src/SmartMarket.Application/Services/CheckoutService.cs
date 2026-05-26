using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Checkout;
using SmartMarket.Application.DTOs.Orders;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Application.Interfaces.Services;
using SmartMarket.Domain.Entities;
using SmartMarket.Domain.Enums;

namespace SmartMarket.Application.Services;

public sealed class CheckoutService(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICheckoutService
{
    public async Task<Result<CheckoutResponse>> CheckoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        Result<CheckoutResponse>? failureResult = null;
        Guid? createdOrderId = null;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var cart = await cartRepository.GetByUserIdWithItemsAsync(userId, ct);
            if (cart is null || cart.Items.Count == 0)
            {
                failureResult = Result<CheckoutResponse>.Failure(
                    ErrorCodes.CheckoutEmptyCart,
                    "Cannot checkout an empty cart.");
                return;
            }

            foreach (var cartItem in cart.Items)
            {
                if (cartItem.Product is null)
                {
                    failureResult = Result<CheckoutResponse>.Failure(
                        ErrorCodes.ProductNotFound,
                        "Product not found.");
                    return;
                }

                if (!cartItem.Product.IsActive)
                {
                    failureResult = Result<CheckoutResponse>.Failure(
                        ErrorCodes.CheckoutProductUnavailable,
                        "Product is not available.");
                    return;
                }

                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                {
                    failureResult = Result<CheckoutResponse>.Failure(
                        ErrorCodes.CheckoutInsufficientStock,
                        "Insufficient stock for checkout.");
                    return;
                }
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0m;

            foreach (var cartItem in cart.Items)
            {
                var lineTotal = cartItem.Quantity * cartItem.UnitPriceSnapshot;
                totalAmount += lineTotal;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = cartItem.ProductId,
                    ProductNameSnapshot = cartItem.Product!.Name,
                    UnitPriceSnapshot = cartItem.UnitPriceSnapshot,
                    Quantity = cartItem.Quantity,
                    LineTotal = lineTotal
                });
            }

            var orderId = Guid.NewGuid();
            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = orderId;
            }

            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                OrderNumber = await GenerateOrderNumberAsync(ct),
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems
            };

            foreach (var cartItem in cart.Items)
            {
                cartItem.Product!.StockQuantity -= cartItem.Quantity;
            }

            await orderRepository.AddAsync(order, ct);
            cart.UpdatedAt = DateTime.UtcNow;
            cartRepository.ClearItems(cart);

            createdOrderId = order.Id;
        }, cancellationToken);

        if (failureResult is not null)
        {
            return failureResult;
        }

        if (createdOrderId is null)
        {
            return Result<CheckoutResponse>.Failure(
                ErrorCodes.ServerError,
                "Checkout failed.");
        }

        var createdOrder = await orderRepository.GetByIdAsync(createdOrderId.Value, cancellationToken);
        if (createdOrder is null)
        {
            return Result<CheckoutResponse>.Failure(
                ErrorCodes.ServerError,
                "Checkout failed.");
        }

        return Result<CheckoutResponse>.Success(new CheckoutResponse(OrderService.MapOrder(createdOrder)));
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var orderNumber = $"SM-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            if (!await orderRepository.OrderNumberExistsAsync(orderNumber, cancellationToken))
            {
                return orderNumber;
            }
        }

        return $"SM-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
    }
}
