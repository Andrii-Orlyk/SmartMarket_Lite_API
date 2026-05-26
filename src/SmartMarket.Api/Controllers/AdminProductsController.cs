using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMarket.Api.Extensions;
using SmartMarket.Application.DTOs.Products;
using SmartMarket.Application.Interfaces.Services;

namespace SmartMarket.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminProductsController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken) =>
        (await productService.CreateProductAsync(request, cancellationToken)).ToActionResult();

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken) =>
        (await productService.UpdateProductAsync(id, request, cancellationToken)).ToActionResult();

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken) =>
        (await productService.DeleteProductAsync(id, cancellationToken)).ToActionResult();
}
