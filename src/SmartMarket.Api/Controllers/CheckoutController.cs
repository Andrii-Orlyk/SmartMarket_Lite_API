using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMarket.Api.Extensions;
using SmartMarket.Application.DTOs.Checkout;
using SmartMarket.Application.Interfaces.Services;

namespace SmartMarket.Api.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize]
public sealed class CheckoutController(ICheckoutService checkoutService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return (await checkoutService.CheckoutAsync(userId.Value, cancellationToken)).ToActionResult();
    }
}
