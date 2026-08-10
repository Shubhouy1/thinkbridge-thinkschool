using LegacyOrders.DTOs;
using LegacyOrders.Models;
using LegacyOrders.Services;
using Microsoft.AspNetCore.Mvc;

namespace LegacyOrders.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderCreateResponse>> CreateOrder([FromBody] OrderRequest? request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateOrder called for customer {CustomerId}", request?.CustomerId ?? 0);

        var response = await _orderService.CreateOrderAsync(request, User.Identity?.Name, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
