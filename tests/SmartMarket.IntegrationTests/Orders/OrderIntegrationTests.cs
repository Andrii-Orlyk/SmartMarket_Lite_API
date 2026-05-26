using System.Net;
using SmartMarket.Application.Common;
using SmartMarket.Domain.Enums;
using SmartMarket.IntegrationTests.Infrastructure;

namespace SmartMarket.IntegrationTests.Orders;

public sealed class OrderIntegrationTests(SmartMarketWebApplicationFactory factory)
    : IClassFixture<SmartMarketWebApplicationFactory>
{
    [Fact]
    public async Task User_GetOwnOrders_ReturnsCheckoutOrder()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, user);

        var response = await user.GetOrdersAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await ApiTestClient.ReadOrdersListAsync(response);
        Assert.Contains(orders, x => x.Id == order.Id);
    }

    [Fact]
    public async Task User_GetOwnOrderById_ReturnsOk()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, user);

        var response = await user.GetOrderByIdAsync(order.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loadedOrder = await ApiTestClient.ReadOrderResponseAsync(response);
        Assert.Equal(order.Id, loadedOrder.Id);
        Assert.Equal(order.OrderNumber, loadedOrder.OrderNumber);
    }

    [Fact]
    public async Task User_GetAnotherUsersOrder_ReturnsNotFound()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var userOne = await TestAuthHelper.CreateUserClientAsync(factory);
        var userTwo = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, userOne);

        var response = await userTwo.GetOrderByIdAsync(order.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.OrderNotFound, error.Code);
    }

    [Fact]
    public async Task Admin_ListOrders_ReturnsOk()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, user);

        var response = await admin.GetAdminOrdersAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await ApiTestClient.ReadAdminOrderPageAsync(response);
        Assert.Contains(page.Items, x => x.Id == order.Id);
    }

    [Fact]
    public async Task Admin_UpdateOrderStatus_ReturnsOk()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, user);

        var response = await admin.UpdateAdminOrderStatusAsync(order.Id, OrderStatus.Paid);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedOrder = await ApiTestClient.ReadOrderResponseAsync(response);
        Assert.Equal(OrderStatus.Paid, updatedOrder.Status);
    }

    [Fact]
    public async Task User_UpdateOrderStatus_ReturnsForbidden()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, user);

        var response = await user.UpdateAdminOrderStatusAsync(order.Id, OrderStatus.Paid);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_GetAdminOrders_ReturnsForbidden()
    {
        var user = await TestAuthHelper.CreateUserClientAsync(factory);

        var response = await user.GetAdminOrdersAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.AuthForbidden, error.Code);
    }

    [Fact]
    public async Task Admin_InvalidOrderStatusTransition_ReturnsConflict()
    {
        var admin = await TestAuthHelper.CreateAdminClientAsync(factory);
        var user = await TestAuthHelper.CreateUserClientAsync(factory);
        var order = await ApiTestClient.CheckoutSingleItemOrderAsync(admin, user);

        var response = await admin.UpdateAdminOrderStatusAsync(order.Id, OrderStatus.Completed);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await ApiTestClient.ReadErrorAsync(response);
        Assert.Equal(ErrorCodes.OrderInvalidStatusTransition, error.Code);
    }
}
