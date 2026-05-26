using SmartMarket.Application.DTOs.Products;
using SmartMarket.Application.Validators.Products;

namespace SmartMarket.UnitTests.Products;

public sealed class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidPrice_Fails(decimal price)
    {
        var request = new CreateProductRequest("Mouse", "Desc", "SKU-001", price, 10, true);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_NegativeStock_Fails()
    {
        var request = new CreateProductRequest("Mouse", "Desc", "SKU-001", 10m, -1, true);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }
}
