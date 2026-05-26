using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Auth;
using SmartMarket.Application.DTOs.Cart;
using SmartMarket.Application.DTOs.Checkout;
using SmartMarket.Application.DTOs.Orders;
using SmartMarket.Application.DTOs.Products;
using SmartMarket.Domain.Enums;

namespace SmartMarket.IntegrationTests.Infrastructure;

public sealed class ApiTestClient(HttpClient client)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<HttpResponseMessage> RegisterAsync(string email, string password = "Password123!") =>
        client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password, "Test", "User"));

    public Task<HttpResponseMessage> LoginAsync(string email, string password = "Password123!") =>
        client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));

    public Task<HttpResponseMessage> GetMeAsync() => client.GetAsync("/api/auth/me");

    public Task<HttpResponseMessage> CreateProductAsync(CreateProductRequest request) =>
        client.PostAsJsonAsync("/api/admin/products", request);

    public Task<HttpResponseMessage> UpdateProductAsync(Guid id, CreateProductRequest request) =>
        client.PutAsJsonAsync($"/api/admin/products/{id}", new UpdateProductRequest(
            request.Name,
            request.Description,
            request.SKU,
            request.Price,
            request.StockQuantity,
            request.IsActive));

    public Task<HttpResponseMessage> GetProductsAsync() => client.GetAsync("/api/products");

    public Task<HttpResponseMessage> GetProductByIdAsync(Guid id) => client.GetAsync($"/api/products/{id}");

    public Task<HttpResponseMessage> GetCartAsync() => client.GetAsync("/api/cart");

    public Task<HttpResponseMessage> AddCartItemAsync(Guid productId, int quantity) =>
        client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(productId, quantity));

    public Task<HttpResponseMessage> UpdateCartItemAsync(Guid cartItemId, int quantity) =>
        client.PutAsJsonAsync($"/api/cart/items/{cartItemId}", new UpdateCartItemQuantityRequest(quantity));

    public Task<HttpResponseMessage> RemoveCartItemAsync(Guid cartItemId) =>
        client.DeleteAsync($"/api/cart/items/{cartItemId}");

    public Task<HttpResponseMessage> ClearCartAsync() =>
        client.DeleteAsync("/api/cart");

    public Task<HttpResponseMessage> CheckoutAsync() =>
        client.PostAsync("/api/checkout", null);

    public Task<HttpResponseMessage> GetOrdersAsync() => client.GetAsync("/api/orders");

    public Task<HttpResponseMessage> GetOrderByIdAsync(Guid orderId) => client.GetAsync($"/api/orders/{orderId}");

    public Task<HttpResponseMessage> GetAdminOrdersAsync(int page = 1, int pageSize = 10) =>
        client.GetAsync($"/api/admin/orders?page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> UpdateAdminOrderStatusAsync(Guid orderId, OrderStatus status) =>
        client.PatchAsJsonAsync($"/api/admin/orders/{orderId}/status", new UpdateOrderStatusRequest(status));

    public void SetBearerToken(string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public void ClearAuthorization() => client.DefaultRequestHeaders.Authorization = null;

    public static async Task<AuthResponse> ReadAuthResponseAsync(HttpResponseMessage response)
    {
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return auth ?? throw new InvalidOperationException("Auth response was empty.");
    }

    public static async Task<ApiErrorResponse> ReadErrorAsync(HttpResponseMessage response)
    {
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        return error ?? throw new InvalidOperationException("Error response was empty.");
    }

    public static async Task<ProductResponse> ReadProductResponseAsync(HttpResponseMessage response)
    {
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);
        return product ?? throw new InvalidOperationException("Product response was empty.");
    }

    public static async Task<PagedResult<ProductResponse>> ReadProductPageAsync(HttpResponseMessage response)
    {
        var page = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>(JsonOptions);
        return page ?? throw new InvalidOperationException("Product page response was empty.");
    }

    public static async Task<CartResponse> ReadCartResponseAsync(HttpResponseMessage response)
    {
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>(JsonOptions);
        return cart ?? throw new InvalidOperationException("Cart response was empty.");
    }

    public static async Task<CheckoutResponse> ReadCheckoutResponseAsync(HttpResponseMessage response)
    {
        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponse>(JsonOptions);
        return checkout ?? throw new InvalidOperationException("Checkout response was empty.");
    }

    public static async Task<IReadOnlyList<OrderResponse>> ReadOrdersListAsync(HttpResponseMessage response)
    {
        var orders = await response.Content.ReadFromJsonAsync<IReadOnlyList<OrderResponse>>(JsonOptions);
        return orders ?? throw new InvalidOperationException("Orders response was empty.");
    }

    public static async Task<OrderResponse> ReadOrderResponseAsync(HttpResponseMessage response)
    {
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        return order ?? throw new InvalidOperationException("Order response was empty.");
    }

    public static async Task<PagedResult<OrderResponse>> ReadAdminOrderPageAsync(HttpResponseMessage response)
    {
        var page = await response.Content.ReadFromJsonAsync<PagedResult<OrderResponse>>(JsonOptions);
        return page ?? throw new InvalidOperationException("Admin orders response was empty.");
    }

    public static async Task<OrderResponse> CheckoutSingleItemOrderAsync(
        ApiTestClient adminClient,
        ApiTestClient userClient,
        int quantity = 1,
        int stockQuantity = 10)
    {
        var product = await CreateActiveProductAsync(adminClient, stockQuantity);
        var addResponse = await userClient.AddCartItemAsync(product.Id, quantity);
        addResponse.EnsureSuccessStatusCode();

        var checkoutResponse = await userClient.CheckoutAsync();
        checkoutResponse.EnsureSuccessStatusCode();

        var checkout = await ReadCheckoutResponseAsync(checkoutResponse);
        return checkout.Order;
    }

    public static async Task<ProductResponse> CreateActiveProductAsync(ApiTestClient adminClient, int stockQuantity = 25)
    {
        var response = await adminClient.CreateProductAsync(BuildProductRequest(stockQuantity: stockQuantity));
        response.EnsureSuccessStatusCode();
        return await ReadProductResponseAsync(response);
    }

    public static CreateProductRequest BuildProductRequest(
        string? sku = null,
        decimal price = 49.99m,
        bool isActive = true,
        int stockQuantity = 25) =>
        new(
            "Wireless Mouse",
            "Compact wireless mouse",
            sku ?? $"SKU-{Guid.NewGuid():N}"[..12],
            price,
            stockQuantity,
            isActive);
}
