
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSP_LAB1.DTOs;
using TSP_LAB1.Interfaces;

namespace TSP_LAB1.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [AllowAnonymous]// TODO: osigurati javni pristup, odnosno, endpoint mora biti dostupan cak i bez autentifikacije
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> Get(CancellationToken ct)
    {
        var result = await products.GetProductsAsync(ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await products.GetByIdAsync(id, ct);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [Authorize(Roles = "Admin")]//TODO: osigurati pristup samo autentificiranom administratoru.
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await products.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }
}
