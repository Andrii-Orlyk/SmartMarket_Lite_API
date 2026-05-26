using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Products;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Product?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Product?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParameters query, bool activeOnly, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    void Update(Product product);

    void Remove(Product product);
}
