using System.Net;
using SmartMarket.Application.Common;
using SmartMarket.IntegrationTests.Infrastructure;

namespace SmartMarket.IntegrationTests.Catalog;

public sealed class CatalogIntegrationTests(SmartMarketWebApplicationFactory factory)
    : IClassFixture<SmartMarketWebApplicationFactory>
{
    [Fact]
    public async Task Admin_CreateProduct_ReturnsOk()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var request = ApiTestClient.BuildProductRequest();

        var response = await admin.CreateProductAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await ApiTestClient.ReadProductResponseAsync(response);
        Assert.Equal(request.SKU, product.SKU);
        Assert.True(product.IsActive);
    }

    [Fact]
    public async Task User_CreateProduct_ReturnsForbidden()
    {
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var request = ApiTestClient.BuildProductRequest();

        var response = await user.CreateProductAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.AuthForbidden, error.Code);
    }

    [Fact]
    public async Task Admin_CreateProduct_InvalidPrice_ReturnsBadRequest()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var request = ApiTestClient.BuildProductRequest(price: 0m);

        var response = await admin.CreateProductAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
    }

    [Fact]
    public async Task Admin_CreateProduct_DuplicateSku_ReturnsConflict()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var sku = $"SKU-{Guid.NewGuid():N}"[..12];
        var request = ApiTestClient.BuildProductRequest(sku: sku);

        var firstResponse = await admin.CreateProductAsync(request);
        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await admin.CreateProductAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(duplicateResponse);
        Assert.Equal(ErrorCodes.ProductSkuExists, error.Code);
    }

    [Fact]
    public async Task PublicProductList_HidesInactiveProduct()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var publicClient = new ApiTestClient(factory.CreateClient());

        var inactiveRequest = ApiTestClient.BuildProductRequest(isActive: false);
        var createResponse = await admin.CreateProductAsync(inactiveRequest);
        createResponse.EnsureSuccessStatusCode();
        var inactiveProduct = await ApiTestClient.ReadProductResponseAsync(createResponse);

        var listResponse = await publicClient.GetProductsAsync();
        listResponse.EnsureSuccessStatusCode();
        var page = await ApiTestClient.ReadProductPageAsync(listResponse);

        Assert.DoesNotContain(page.Items, x => x.Id == inactiveProduct.Id);

        var byIdResponse = await publicClient.GetProductByIdAsync(inactiveProduct.Id);
        Assert.Equal(HttpStatusCode.NotFound, byIdResponse.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(byIdResponse);
        Assert.Equal(ErrorCodes.ProductNotFound, error.Code);
    }
}
