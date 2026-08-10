using Microsoft.EntityFrameworkCore;

namespace LegacyOrders.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}
