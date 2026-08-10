using LegacyOrders.Models;

namespace LegacyOrders.Repositories;

public interface IOrderRepository
{
    Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken);

    Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken);

    Task<Order> CreateOrderAsync(Order order, Customer customer, IEnumerable<Product> products, string? userName, CancellationToken cancellationToken);
}
