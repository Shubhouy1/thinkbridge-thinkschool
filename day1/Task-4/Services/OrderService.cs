using LegacyOrders.DTOs;
using LegacyOrders.Models;
using LegacyOrders.Repositories;

namespace LegacyOrders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;
    private readonly IShippingStrategy _shippingStrategy;
    private readonly ITaxStrategy _taxStrategy;
    private readonly IDiscountStrategy _discountStrategy;

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
        : this(repository, logger, new DefaultShippingStrategy(), new DefaultTaxStrategy(), new DefaultDiscountStrategy())
    {
    }

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger, IShippingStrategy shippingStrategy, ITaxStrategy taxStrategy, IDiscountStrategy discountStrategy)
    {
        _repository = repository;
        _logger = logger;
        _shippingStrategy = shippingStrategy;
        _taxStrategy = taxStrategy;
        _discountStrategy = discountStrategy;
    }

    public async Task<OrderCreateResponse> CreateOrderAsync(OrderRequest? request, string? userName, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return CreateFailure("Request is required");
        }

        if (request.CustomerId <= 0)
        {
            return CreateFailure("Invalid customer");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return CreateFailure("Order must contain items");
        }

        foreach (var item in request.Items)
        {
            if (item.ProductId <= 0)
            {
                return CreateFailure("Invalid product");
            }

            if (item.Quantity <= 0)
            {
                return CreateFailure("Quantity must be greater than zero");
            }
        }

        var customer = await _repository.GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return CreateFailure("Customer not found");
        }

        var products = await _repository.GetProductsAsync(cancellationToken);
        var productLookup = products.ToDictionary(x => x.Id, x => x);

        decimal subtotal = 0m;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            if (!productLookup.TryGetValue(item.ProductId, out var product))
            {
                return CreateFailure("Product not found");
            }

            if (product.Stock < item.Quantity)
            {
                return CreateFailure("Not enough stock");
            }

            var itemTotal = product.Price * item.Quantity;
            if (item.Quantity > 10)
            {
                itemTotal *= 0.90m;
            }

            if (customer.IsVip)
            {
                itemTotal *= 0.95m;
            }

            subtotal += itemTotal;
            product.Stock -= item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Total = itemTotal
            });
        }

        decimal shipping = CalculateShipping(subtotal, request.Country);
        decimal tax = CalculateTax(subtotal, customer.State);
        decimal discount = CalculateDiscount(subtotal, request.CouponCode, customer.IsVip);
        decimal total = subtotal + shipping + tax - discount;
        if (total < 0)
        {
            total = 0m;
        }

        var order = new Order
        {
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            Status = total > 1000m ? "HighValue" : "Created",
            SubTotal = subtotal,
            Shipping = shipping,
            Tax = tax,
            Discount = discount,
            Total = total,
            Items = orderItems
        };

        customer.LastOrderDate = DateTime.UtcNow;
        customer.TotalOrders++;
        customer.TotalSpent += total;

        if (customer.IsVip && total > 500m)
        {
            customer.VipPoints += (int)total;
        }

        if (request.SendEmail)
        {
            _logger.LogInformation("Email requested for order for customer {CustomerId}", customer.Id);
        }

        var createdOrder = await _repository.CreateOrderAsync(order, customer, products, userName, cancellationToken);

        var responseItems = createdOrder.Items.Select(x => new OrderItemResponse
        {
            ProductId = x.ProductId,
            ProductName = productLookup[x.ProductId].Name,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Total = x.Total
        }).ToList();

        _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", createdOrder.Id, customer.Id);

        return new OrderCreateResponse
        {
            Success = true,
            OrderId = createdOrder.Id,
            Customer = new OrderCustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email
            },
            Subtotal = subtotal,
            Shipping = shipping,
            Tax = tax,
            Discount = discount,
            Total = total,
            Status = createdOrder.Status,
            Items = responseItems
        };
    }

    private decimal CalculateShipping(decimal subtotal, string country)
    {
        return _shippingStrategy.Calculate(subtotal, country);
    }

    private decimal CalculateTax(decimal subtotal, string state)
    {
        return _taxStrategy.Calculate(subtotal, state);
    }

    private decimal CalculateDiscount(decimal subtotal, string couponCode, bool isVip)
    {
        return _discountStrategy.Calculate(subtotal, couponCode, isVip);
    }

    private static OrderCreateResponse CreateFailure(string message)
    {
        return new OrderCreateResponse
        {
            Success = false,
            Message = message
        };
    }
}
