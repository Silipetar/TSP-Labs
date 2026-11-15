
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSP_LAB1.DTOs;
using TSP_LAB1.Interfaces;
using System.Security.Claims;

namespace TSP_LAB1.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(
    IOrderRepository orderRepository,
    IOrderService orderService,
    IAuthorizationService authorizationService)
    : ControllerBase
{

    // TODO: osigurati da samo administrator može vidjeti sve narudzbe.
    // TODO: osigurati da autentificirani korisnik može vidjeti samo vlastite narudzbe.
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAll(CancellationToken ct)
    {
        var orders = await orderRepository.GetAllAsync(ct);

        // Administrator vidi sve, ostali samo vlastite narudžbe.
        if (User.FindFirstValue("role") != "Admin")
        {
            if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId))
                return Unauthorized();

            orders = orders.Where(o => o.CustomerId == userId).ToList();
        }

        return Ok(orders.Select(o => new OrderResponse(o.Id, o.Description, o.Total, o.CreatedAt)).ToList());
    }

    // TODO: osigurati da autentificirani korisnik vidi samo svoju narudzbu.
    // TODO osigurati da samo administrator može vidjeti svaku narudzbu.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken ct)
    {
        // 1. Dohvat resursa.
        var order = await orderRepository.GetByIdAsync(id, ct);

        if (order is null)
            return NotFound();

        // 2. TODO: unutar Autorizacijske politike osigurati provjeru prava pristupa resursu
        var authorizationResult = await authorizationService.AuthorizeAsync(User, order, "OrderOwner");
        if (!authorizationResult.Succeeded)
        {
            // TODO: osigurati odgovarajuci Error Status Response umjesto OK.
            return NotFound();
        }

        // 3. Tek nakon autorizacije vraćamo podatke.
        var response = new OrderResponse(order.Id, order.Description, order.Total, order.CreatedAt);
        return Ok(response);
    }
}
