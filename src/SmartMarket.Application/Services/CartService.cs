using FluentValidation;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Cart;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Application.Interfaces.Services;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Application.Services;

public sealed class CartService(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IValidator<AddCartItemRequest> addItemValidator,
    IValidator<UpdateCartItemQuantityRequest> updateQuantityValidator) : ICartService
{
    public async Task<Result<CartResponse>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetByUserIdWithItemsAsync(userId, cancellationToken);
        return Result<CartResponse>.Success(Map(cart, userId));
    }

    public async Task<Result<CartResponse>> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await addItemValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CartResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<CartResponse>.Failure(ErrorCodes.ProductNotFound, "Product not found.");
        }

        if (!product.IsActive)
        {
            return Result<CartResponse>.Failure(ErrorCodes.ProductInactive, "Product is inactive.");
        }

        if (product.StockQuantity <= 0)
        {
            return Result<CartResponse>.Failure(
                ErrorCodes.CheckoutProductUnavailable,
                "Product is not available.");
        }

        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        var targetQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (targetQuantity > product.StockQuantity)
        {
            return Result<CartResponse>.Failure(
                ErrorCodes.CheckoutInsufficientStock,
                "Insufficient stock for the requested quantity.");
        }

        var utcNow = DateTime.UtcNow;
        cart.UpdatedAt = utcNow;

        if (existingItem is not null)
        {
            existingItem.Quantity = targetQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
                UnitPriceSnapshot = product.Price
            };

            cart.Items.Add(cartItem);
            await cartRepository.AddItemAsync(cartItem, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cart = (await cartRepository.GetByUserIdWithItemsAsync(userId, cancellationToken))!;

        return Result<CartResponse>.Success(Map(cart, userId));
    }

    public async Task<Result<CartResponse>> UpdateItemQuantityAsync(
        Guid userId,
        Guid cartItemId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateQuantityValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CartResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var cartItem = await cartRepository.GetCartItemByIdAsync(cartItemId, cancellationToken);
        if (cartItem is null || cartItem.Cart.UserId != userId)
        {
            return Result<CartResponse>.Failure(ErrorCodes.CartItemNotFound, "Cart item not found.");
        }

        var product = await productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<CartResponse>.Failure(ErrorCodes.ProductNotFound, "Product not found.");
        }

        if (!product.IsActive)
        {
            return Result<CartResponse>.Failure(ErrorCodes.ProductInactive, "Product is inactive.");
        }

        if (product.StockQuantity <= 0)
        {
            return Result<CartResponse>.Failure(
                ErrorCodes.CheckoutProductUnavailable,
                "Product is not available.");
        }

        if (request.Quantity > product.StockQuantity)
        {
            return Result<CartResponse>.Failure(
                ErrorCodes.CheckoutInsufficientStock,
                "Insufficient stock for the requested quantity.");
        }

        cartItem.Quantity = request.Quantity;
        cartItem.Cart.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cart = (await cartRepository.GetByUserIdWithItemsAsync(userId, cancellationToken))!;
        return Result<CartResponse>.Success(Map(cart, userId));
    }

    public async Task<Result<CartResponse>> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        CancellationToken cancellationToken = default)
    {
        var cartItem = await cartRepository.GetCartItemByIdAsync(cartItemId, cancellationToken);
        if (cartItem is null || cartItem.Cart.UserId != userId)
        {
            return Result<CartResponse>.Failure(ErrorCodes.CartItemNotFound, "Cart item not found.");
        }

        cartItem.Cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.RemoveItem(cartItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cart = await cartRepository.GetByUserIdWithItemsAsync(userId, cancellationToken);
        return Result<CartResponse>.Success(Map(cart, userId));
    }

    public async Task<Result<CartResponse>> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetByUserIdWithItemsAsync(userId, cancellationToken);
        if (cart is null)
        {
            return Result<CartResponse>.Success(Map(null, userId));
        }

        cart.UpdatedAt = DateTime.UtcNow;
        cartRepository.ClearItems(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartResponse>.Success(Map(cart, userId));
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdWithItemsAsync(userId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        var utcNow = DateTime.UtcNow;
        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await cartRepository.AddAsync(cart, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return cart;
    }

    private static CartResponse Map(Cart? cart, Guid userId)
    {
        if (cart is null)
        {
            var emptyTimestamp = DateTime.UtcNow;
            return new CartResponse(
                Guid.Empty,
                userId,
                Array.Empty<CartItemResponse>(),
                0m,
                emptyTimestamp,
                emptyTimestamp);
        }

        var items = cart.Items
            .Select(item => new CartItemResponse(
                item.Id,
                item.ProductId,
                item.Product?.Name ?? string.Empty,
                item.Product?.SKU ?? string.Empty,
                item.Quantity,
                item.UnitPriceSnapshot,
                item.Quantity * item.UnitPriceSnapshot))
            .ToList();

        return new CartResponse(
            cart.Id,
            cart.UserId,
            items,
            items.Sum(x => x.LineTotal),
            cart.CreatedAt,
            cart.UpdatedAt);
    }
}
