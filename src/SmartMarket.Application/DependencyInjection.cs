using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartMarket.Application.Interfaces.Services;
using SmartMarket.Application.Services;
using SmartMarket.Application.Validators.Auth;
using SmartMarket.Application.Validators.Cart;
using SmartMarket.Application.Validators.Products;

namespace SmartMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
