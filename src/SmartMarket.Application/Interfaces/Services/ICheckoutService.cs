using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Checkout;

namespace SmartMarket.Application.Interfaces.Services;

public interface ICheckoutService
{
    Task<Result<CheckoutResponse>> CheckoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
