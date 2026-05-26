using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Cart;

namespace SmartMarket.Application.Interfaces.Services;

public interface ICartService
{
    Task<Result<CartResponse>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<CartResponse>> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken = default);

    Task<Result<CartResponse>> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken = default);

    Task<Result<CartResponse>> RemoveItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default);

    Task<Result<CartResponse>> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
