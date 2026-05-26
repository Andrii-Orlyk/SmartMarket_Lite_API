using Microsoft.EntityFrameworkCore;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Domain.Entities;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.Infrastructure.Repositories;

public sealed class CartRepository(SmartMarketDbContext context) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Carts
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default) =>
        await context.Carts.AddAsync(cart, cancellationToken);

    public Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId, CancellationToken cancellationToken = default) =>
        context.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == cartItemId, cancellationToken);

    public async Task AddItemAsync(CartItem cartItem, CancellationToken cancellationToken = default) =>
        await context.CartItems.AddAsync(cartItem, cancellationToken);

    public void RemoveItem(CartItem cartItem) => context.CartItems.Remove(cartItem);

    public void ClearItems(Cart cart)
    {
        if (cart.Items.Count == 0)
        {
            return;
        }

        context.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
    }
}
