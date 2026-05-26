namespace SmartMarket.Application.DTOs.Products;

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    string SKU,
    decimal Price,
    int StockQuantity,
    bool IsActive);
