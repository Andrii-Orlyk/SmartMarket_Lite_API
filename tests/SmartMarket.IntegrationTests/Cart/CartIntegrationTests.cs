using System.Net;
using SmartMarket.Application.Common;
using SmartMarket.IntegrationTests.Infrastructure;

namespace SmartMarket.IntegrationTests.Cart;

public sealed class CartIntegrationTests(SmartMarketWebApplicationFactory factory)
    : IClassFixture<SmartMarketWebApplicationFactory>
{
    [Fact]
    public async Task User_AddProductToCart_ReturnsOk()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin);

        var response = await user.AddCartItemAsync(product.Id, 2);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await ApiTestClient.ReadCartResponseAsync(response);
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].Quantity);
        Assert.Equal(product.Price, cart.Items[0].UnitPriceSnapshot);
        Assert.Equal(product.Price * 2, cart.TotalAmount);
    }

    [Fact]
    public async Task User_AddSameProductTwice_IncreasesQuantity()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin);

        await user.AddCartItemAsync(product.Id, 2);
        var response = await user.AddCartItemAsync(product.Id, 3);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await ApiTestClient.ReadCartResponseAsync(response);
        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items[0].Quantity);
        Assert.Equal(product.Price * 5, cart.Items[0].LineTotal);
    }

    [Fact]
    public async Task User_UpdateCartItemQuantity_ReturnsOk()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin);

        var addResponse = await user.AddCartItemAsync(product.Id, 2);
        addResponse.EnsureSuccessStatusCode();
        var addedCart = await ApiTestClient.ReadCartResponseAsync(addResponse);
        var cartItemId = addedCart.Items[0].Id;

        var response = await user.UpdateCartItemAsync(cartItemId, 4);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await ApiTestClient.ReadCartResponseAsync(response);
        Assert.Equal(4, cart.Items[0].Quantity);
        Assert.Equal(addedCart.Items[0].UnitPriceSnapshot, cart.Items[0].UnitPriceSnapshot);
    }

    [Fact]
    public async Task User_AddItem_InvalidQuantity_ReturnsBadRequest()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin);

        var response = await user.AddCartItemAsync(product.Id, 0);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
    }

    [Fact]
    public async Task User_AddInactiveProduct_ReturnsConflict()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);

        var createResponse = await admin.CreateProductAsync(
            ApiTestClient.BuildProductRequest(isActive: false));
        createResponse.EnsureSuccessStatusCode();
        var inactiveProduct = await ApiTestClient.ReadProductResponseAsync(createResponse);

        var response = await user.AddCartItemAsync(inactiveProduct.Id, 1);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.ProductInactive, error.Code);
    }

    [Fact]
    public async Task User_RemoveCartItem_ReturnsCartWithoutThatItem()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var firstProduct = await ApiTestClient.CreateActiveProductAsync(admin);
        var secondProduct = await ApiTestClient.CreateActiveProductAsync(admin);

        var firstAddResponse = await user.AddCartItemAsync(firstProduct.Id, 1);
        firstAddResponse.EnsureSuccessStatusCode();
        var secondAddResponse = await user.AddCartItemAsync(secondProduct.Id, 2);
        secondAddResponse.EnsureSuccessStatusCode();
        var cartWithTwoItems = await ApiTestClient.ReadCartResponseAsync(secondAddResponse);
        var cartItemToRemove = cartWithTwoItems.Items.Single(x => x.ProductId == firstProduct.Id).Id;

        var response = await user.RemoveCartItemAsync(cartItemToRemove);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await ApiTestClient.ReadCartResponseAsync(response);
        Assert.Single(cart.Items);
        Assert.Equal(secondProduct.Id, cart.Items[0].ProductId);
        Assert.Equal(2, cart.Items[0].Quantity);
        Assert.DoesNotContain(cart.Items, x => x.ProductId == firstProduct.Id);
    }

    [Fact]
    public async Task User_ClearCart_ReturnsEmptyCart()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin);

        var addResponse = await user.AddCartItemAsync(product.Id, 2);
        addResponse.EnsureSuccessStatusCode();

        var response = await user.ClearCartAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await ApiTestClient.ReadCartResponseAsync(response);
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.TotalAmount);
    }
}
