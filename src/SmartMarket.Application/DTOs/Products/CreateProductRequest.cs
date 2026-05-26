namespace SmartMarket.Application.DTOs.Products;

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    string SKU,
    decimal Price,
    int StockQuantity,
    bool IsActive);
