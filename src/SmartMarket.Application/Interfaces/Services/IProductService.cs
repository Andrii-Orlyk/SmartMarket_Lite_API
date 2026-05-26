using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Products;

namespace SmartMarket.Application.Interfaces.Services;

public interface IProductService
{
    Task<Result<PagedResult<ProductResponse>>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken = default);

    Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}
