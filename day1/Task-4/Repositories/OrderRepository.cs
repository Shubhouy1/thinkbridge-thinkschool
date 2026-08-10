using LegacyOrders.Models;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrders.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext db, ILogger<OrderRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        return await _db.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
    }

    public async Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken)
    {
        return await _db.Products.ToListAsync(cancellationToken);
    }

    public async Task<Order> CreateOrderAsync(Order order, Customer customer, IEnumerable<Product> products, string? userName, CancellationToken cancellationToken)
    {
        try
        {
            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            _db.Orders.Add(order);
            _db.Customers.Update(customer);

            foreach (var product in products)
            {
                _db.Products.Update(product);
            }

            await _db.SaveChangesAsync(cancellationToken);

            var history = new OrderHistory
            {
                OrderId = order.Id,
                Action = "Order created",
                CreatedAt = DateTime.UtcNow
            };

            _db.OrderHistories.Add(history);

            var audit = new AuditLog
            {
                EntityId = order.Id,
                EntityName = "Order",
                Action = "CREATE",
                UserName = userName ?? "anonymous",
                CreatedAt = DateTime.UtcNow
            };

            _db.AuditLogs.Add(audit);

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return order;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save order for customer {CustomerId}", customer.Id);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Order persistence cancelled for customer {CustomerId}", customer.Id);
            throw;
        }
    }
}
