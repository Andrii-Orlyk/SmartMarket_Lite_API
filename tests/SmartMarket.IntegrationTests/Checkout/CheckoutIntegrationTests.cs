using System.Net;
using SmartMarket.Application.Common;
using SmartMarket.Domain.Enums;
using SmartMarket.IntegrationTests.Infrastructure;

namespace SmartMarket.IntegrationTests.Checkout;

public sealed class CheckoutIntegrationTests(SmartMarketWebApplicationFactory factory)
    : IClassFixture<SmartMarketWebApplicationFactory>
{
    [Fact]
    public async Task Checkout_ValidCart_CreatesOrderWithSnapshots_DecreasesStock_AndClearsCart()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin, stockQuantity: 10);

        var addResponse = await user.AddCartItemAsync(product.Id, 2);
        addResponse.EnsureSuccessStatusCode();
        var cartBeforeCheckout = await ApiTestClient.ReadCartResponseAsync(addResponse);
        var unitPriceSnapshot = cartBeforeCheckout.Items[0].UnitPriceSnapshot;

        var checkoutResponse = await user.CheckoutAsync();

        Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
        var checkout = await ApiTestClient.ReadCheckoutResponseAsync(checkoutResponse);
        var order = checkout.Order;

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(product.Name, order.Items[0].ProductNameSnapshot);
        Assert.Equal(unitPriceSnapshot, order.Items[0].UnitPriceSnapshot);
        Assert.Equal(2, order.Items[0].Quantity);
        Assert.Equal(unitPriceSnapshot * 2, order.Items[0].LineTotal);
        Assert.Equal(unitPriceSnapshot * 2, order.TotalAmount);

        var cartResponse = await user.GetCartAsync();
        cartResponse.EnsureSuccessStatusCode();
        var clearedCart = await ApiTestClient.ReadCartResponseAsync(cartResponse);
        Assert.Empty(clearedCart.Items);
        Assert.Equal(0m, clearedCart.TotalAmount);

        var productResponse = await user.GetProductByIdAsync(product.Id);
        productResponse.EnsureSuccessStatusCode();
        var updatedProduct = await ApiTestClient.ReadProductResponseAsync(productResponse);
        Assert.Equal(8, updatedProduct.StockQuantity);
    }

    [Fact]
    public async Task Checkout_EmptyCart_ReturnsConflict()
    {
        var user = await TestAuthHelper.CreateUserClientAsync(factory);

        var response = await user.CheckoutAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.CheckoutEmptyCart, error.Code);
    }

    [Fact]
    public async Task Checkout_InsufficientStock_ReturnsConflict()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var product = await ApiTestClient.CreateActiveProductAsync(admin, stockQuantity: 3);

        await user.AddCartItemAsync(product.Id, 3);

        var updateRequest = ApiTestClient.BuildProductRequest(
            sku: product.SKU,
            price: product.Price,
            isActive: true,
            stockQuantity: 1);
        var adminUpdateResponse = await admin.UpdateProductAsync(product.Id, updateRequest);
        adminUpdateResponse.EnsureSuccessStatusCode();

        var response = await user.CheckoutAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.CheckoutInsufficientStock, error.Code);
    }

    [Fact]
    public async Task Checkout_OrderItem_KeepsCartUnitPriceSnapshot_WhenProductPriceChangesBeforeCheckout()
    {
        const decimal priceAtAddToCart = 49.99m;
        const decimal updatedProductPrice = 99.99m;

        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);

        var createResponse = await admin.CreateProductAsync(
            ApiTestClient.BuildProductRequest(price: priceAtAddToCart));
        createResponse.EnsureSuccessStatusCode();
        var product = await ApiTestClient.ReadProductResponseAsync(createResponse);

        var addResponse = await user.AddCartItemAsync(product.Id, 1);
        addResponse.EnsureSuccessStatusCode();
        var cartBeforeCheckout = await ApiTestClient.ReadCartResponseAsync(addResponse);
        Assert.Equal(priceAtAddToCart, cartBeforeCheckout.Items[0].UnitPriceSnapshot);

        var updateRequest = ApiTestClient.BuildProductRequest(
            sku: product.SKU,
            price: updatedProductPrice,
            isActive: true,
            stockQuantity: product.StockQuantity);
        var updateResponse = await admin.UpdateProductAsync(product.Id, updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var checkoutResponse = await user.CheckoutAsync();

        Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
        var checkout = await ApiTestClient.ReadCheckoutResponseAsync(checkoutResponse);
        Assert.Equal(priceAtAddToCart, checkout.Order.Items[0].UnitPriceSnapshot);
        Assert.NotEqual(updatedProductPrice, checkout.Order.Items[0].UnitPriceSnapshot);
        Assert.Equal(priceAtAddToCart, checkout.Order.Items[0].LineTotal);
    }
}
