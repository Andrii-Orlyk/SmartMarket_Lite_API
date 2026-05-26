using Microsoft.EntityFrameworkCore;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Products;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Domain.Entities;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.Infrastructure.Repositories;

public sealed class ProductRepository(SmartMarketDbContext context) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Product?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public Task<Product?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        context.Products
            .FirstOrDefaultAsync(x => x.SKU == sku, cancellationToken);

    public Task<bool> ExistsBySkuAsync(
        string sku,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Products.AsNoTracking().Where(x => x.SKU == sku);
        if (excludeProductId.HasValue)
        {
            query = query.Where(x => x.Id != excludeProductId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        ProductQueryParameters query,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 100);

        var products = context.Products.AsNoTracking().AsQueryable();

        if (activeOnly)
        {
            products = products.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(x =>
                x.Name.Contains(search) ||
                x.SKU.Contains(search) ||
                (x.Description != null && x.Description.Contains(search)));
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(x => x.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(x => x.Price <= query.MaxPrice.Value);
        }

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await products
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await context.Products.AddAsync(product, cancellationToken);

    public void Update(Product product) => context.Products.Update(product);

    public void Remove(Product product) => context.Products.Remove(product);
}
