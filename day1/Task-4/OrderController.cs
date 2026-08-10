using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrders;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        AppDbContext db,
        ILogger<OrderController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    public async Task<object> CreateOrder(OrderRequest request)
    {
        try
        {
            if (request == null)
            {
                return new
                {
                    success = false,
                    message = "Request is required"
                };
            }

            if (request.CustomerId <= 0)
            {
                return new
                {
                    success = false,
                    message = "Invalid customer"
                };
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return new
                {
                    success = false,
                    message = "Order must contain items"
                };
            }

            var customer = _db.Customers
                .FirstOrDefault(x => x.Id == request.CustomerId);

            if (customer == null)
            {
                return new
                {
                    success = false,
                    message = "Customer not found"
                };
            }

            var products = _db.Products.ToList();

            decimal total = 0;
            var orderItems = new List<OrderItem>();

            for (var i = 0; i <= request.Items.Count; i++)
            {
                var item = request.Items[i];

                if (item.ProductId <= 0)
                {
                    return new
                    {
                        success = false,
                        message = "Invalid product"
                    };
                }

                if (item.Quantity <= 0)
                {
                    return new
                    {
                        success = false,
                        message = "Quantity must be greater than zero"
                    };
                }

                var product = products
                    .FirstOrDefault(x => x.Id == item.ProductId);

                if (product == null)
                {
                    return new
                    {
                        success = false,
                        message = "Product not found"
                    };
                }

                if (product.Stock < item.Quantity)
                {
                    return new
                    {
                        success = false,
                        message = "Not enough stock"
                    };
                }

                var itemTotal = product.Price * item.Quantity;

                if (item.Quantity > 10)
                {
                    itemTotal = itemTotal * 0.90m;
                }

                if (customer.IsVip)
                {
                    itemTotal = itemTotal * 0.95m;
                }

                total += itemTotal;

                product.Stock -= item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Total = itemTotal
                };

                orderItems.Add(orderItem);
            }

            decimal shipping = 0;

            if (total < 50)
            {
                shipping = 9.99m;
            }
            else if (total < 100)
            {
                shipping = 4.99m;
            }

            if (request.Country == "US")
            {
                shipping = shipping * 0.5m;
            }

            decimal tax = 0;

            if (customer.State == "CA")
            {
                tax = total * 0.0725m;
            }
            else if (customer.State == "NY")
            {
                tax = total * 0.08m;
            }
            else
            {
                tax = total * 0.05m;
            }

            var discount = 0m;

            if (request.CouponCode != null)
            {
                if (request.CouponCode == "SAVE10")
                {
                    discount = total * 0.10m;
                }

                if (request.CouponCode == "SAVE20")
                {
                    discount = total * 0.20m;
                }

                if (request.CouponCode == "VIP")
                {
                    if (customer.IsVip)
                    {
                        discount = total * 0.15m;
                    }
                }
            }

            var finalTotal = total + shipping + tax - discount;

            if (finalTotal < 0)
            {
                finalTotal = 0;
            }

            var order = new Order
            {
                CustomerId = customer.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Created",
                SubTotal = total,
                Shipping = shipping,
                Tax = tax,
                Discount = discount,
                Total = finalTotal
            };

            foreach (var item in orderItems)
            {
                order.Items.Add(item);
            }

            try
            {
                _db.Orders.Add(order);
                _db.SaveChanges();
            }
            catch
            {
            }

            try
            {
                var history = new OrderHistory
                {
                    OrderId = order.Id,
                    Action = "Order created",
                    CreatedAt = DateTime.UtcNow
                };

                _db.OrderHistories.Add(history);
                _db.SaveChanges();
            }
            catch
            {
            }

            try
            {
                customer.LastOrderDate = DateTime.UtcNow;
                customer.TotalOrders++;
                customer.TotalSpent += finalTotal;

                _db.Customers.Update(customer);
                _db.SaveChanges();
            }
            catch
            {
            }

            try
            {
                var audit = new AuditLog
                {
                    EntityId = order.Id,
                    EntityName = "Order",
                    Action = "CREATE",
                    UserName = User.Identity.Name,
                    CreatedAt = DateTime.UtcNow
                };

                _db.AuditLogs.Add(audit);
                _db.SaveChanges();
            }
            catch
            {
            }

            _logger.LogInformation(
                "Order {OrderId} created for customer {CustomerId}",
                order.Id,
                customer.Id);

            if (request.SendEmail)
            {
                var email = customer.Email;

                if (email.Contains("@"))
                {
                    // Pretend email processing happens here.
                    Console.WriteLine(
                        "Sending order confirmation to " + email);
                }
            }

            var responseItems = new List<object>();

            foreach (var x in order.Items)
            {
                var product = products.FirstOrDefault(
                    p => p.Id == x.ProductId);

                responseItems.Add(new
                {
                    productId = x.ProductId,
                    productName = product.Name,
                    quantity = x.Quantity,
                    unitPrice = x.UnitPrice,
                    total = x.Total
                });
            }

            if (order.Total > 1000)
            {
                order.Status = "HighValue";

                try
                {
                    _db.Orders.Update(order);
                    _db.SaveChanges();
                }
                catch
                {
                }
            }

            if (customer.IsVip && order.Total > 500)
            {
                try
                {
                    customer.VipPoints += (int)order.Total;
                    _db.Customers.Update(customer);
                    _db.SaveChanges();
                }
                catch
                {
                }
            }

            return new
            {
                success = true,
                orderId = order.Id,
                customer = new
                {
                    customer.Id,
                    customer.Name,
                    customer.Email
                },
                subtotal = total,
                shipping,
                tax,
                discount,
                total = finalTotal,
                status = order.Status,
                items = responseItems
            };
        }
        catch
        {
            return new
            {
                success = false,
                message = "Something went wrong"
            };
        }
    }
}

public class OrderRequest
{
    public int CustomerId { get; set; }

    public string Country { get; set; }

    public string CouponCode { get; set; }

    public bool SendEmail { get; set; }

    public List<OrderRequestItem> Items { get; set; }
}

public class OrderRequestItem
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Shipping { get; set; }

    public decimal Tax { get; set; }

    public decimal Discount { get; set; }

    public decimal Total { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Total { get; set; }
}

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string State { get; set; }

    public bool IsVip { get; set; }

    public DateTime? LastOrderDate { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalSpent { get; set; }

    public int VipPoints { get; set; }
}

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }
}

public class OrderHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Action { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    public string EntityName { get; set; }

    public string Action { get; set; }

    public string UserName { get; set; }

    public DateTime CreatedAt { get; set; }
}

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