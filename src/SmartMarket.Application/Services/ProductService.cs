using FluentValidation;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Products;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Application.Interfaces.Services;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Application.Services;

public sealed class ProductService(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator) : IProductService
{
    public async Task<Result<PagedResult<ProductResponse>>> GetProductsAsync(
        ProductQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var page = await productRepository.GetPagedAsync(query, activeOnly: true, cancellationToken);
        return Result<PagedResult<ProductResponse>>.Success(Map(page));
    }

    public async Task<Result<ProductResponse>> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetActiveByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductResponse>.Failure(
                ErrorCodes.ProductNotFound,
                "Product not found.");
        }

        return Result<ProductResponse>.Success(Map(product));
    }

    public async Task<Result<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ProductResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var sku = NormalizeSku(request.SKU);
        if (await productRepository.ExistsBySkuAsync(sku, cancellationToken: cancellationToken))
        {
            return Result<ProductResponse>.Failure(
                ErrorCodes.ProductSkuExists,
                "Product SKU already exists.");
        }

        var utcNow = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            SKU = sku,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductResponse>.Success(Map(product));
    }

    public async Task<Result<ProductResponse>> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ProductResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var product = await productRepository.GetTrackedByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductResponse>.Failure(
                ErrorCodes.ProductNotFound,
                "Product not found.");
        }

        var sku = NormalizeSku(request.SKU);
        if (await productRepository.ExistsBySkuAsync(sku, id, cancellationToken))
        {
            return Result<ProductResponse>.Failure(
                ErrorCodes.ProductSkuExists,
                "Product SKU already exists.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.SKU = sku;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductResponse>.Success(Map(product));
    }

    public async Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetTrackedByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(
                ErrorCodes.ProductNotFound,
                "Product not found.");
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string NormalizeSku(string sku) => sku.Trim();

    private static ProductResponse Map(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.SKU,
            product.Price,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);

    private static PagedResult<ProductResponse> Map(PagedResult<Product> page) =>
        new()
        {
            Items = page.Items.Select(Map).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
}
