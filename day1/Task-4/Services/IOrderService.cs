using LegacyOrders.DTOs;
using LegacyOrders.Models;

namespace LegacyOrders.Services;

public interface IOrderService
{
    Task<OrderCreateResponse> CreateOrderAsync(OrderRequest? request, string? userName, CancellationToken cancellationToken);
}
