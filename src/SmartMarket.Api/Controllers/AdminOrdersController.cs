using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMarket.Api.Extensions;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Orders;
using SmartMarket.Application.Interfaces.Services;

namespace SmartMarket.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminOrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        (await orderService.GetAllOrdersAsync(page, pageSize, cancellationToken)).ToActionResult();

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken) =>
        (await orderService.UpdateOrderStatusAsync(id, request, cancellationToken)).ToActionResult();
}
