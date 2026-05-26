using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMarket.Api.Extensions;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Products;
using SmartMarket.Application.Interfaces.Services;

namespace SmartMarket.Api.Controllers;

[ApiController]
[Route("api/products")]
[AllowAnonymous]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] ProductQueryParameters query,
        CancellationToken cancellationToken) =>
        (await productService.GetProductsAsync(query, cancellationToken)).ToActionResult();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken) =>
        (await productService.GetProductByIdAsync(id, cancellationToken)).ToActionResult();
}
