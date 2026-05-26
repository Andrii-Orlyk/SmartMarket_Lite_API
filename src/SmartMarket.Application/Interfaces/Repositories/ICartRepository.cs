using SmartMarket.Domain.Entities;

namespace SmartMarket.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);

    Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId, CancellationToken cancellationToken = default);

    Task AddItemAsync(CartItem cartItem, CancellationToken cancellationToken = default);

    void RemoveItem(CartItem cartItem);

    void ClearItems(Cart cart);
}
