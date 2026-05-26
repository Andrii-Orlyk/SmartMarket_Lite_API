using Microsoft.EntityFrameworkCore;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Infrastructure.Persistence;

public sealed class SmartMarketDbContext(DbContextOptions<SmartMarketDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartMarketDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
