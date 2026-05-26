namespace SmartMarket.Application.DTOs.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    string SKU,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
