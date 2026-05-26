using SmartMarket.Application.DTOs.Cart;
using SmartMarket.Application.Validators.Cart;

namespace SmartMarket.UnitTests.Cart;

public sealed class CartRequestValidatorTests
{
    [Fact]
    public async Task AddCartItemValidator_ZeroQuantity_Fails()
    {
        var validator = new AddCartItemRequestValidator();
        var request = new AddCartItemRequest(Guid.NewGuid(), 0);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task UpdateCartItemValidator_ZeroQuantity_Fails()
    {
        var validator = new UpdateCartItemQuantityRequestValidator();
        var request = new UpdateCartItemQuantityRequest(0);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }
}
