namespace SmartMarket.Application.DTOs.Products;

public sealed class ProductQueryParameters
{
    public string? Search { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
