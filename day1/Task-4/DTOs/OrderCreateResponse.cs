namespace LegacyOrders.DTOs;

public class OrderCreateResponse
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public int? OrderId { get; init; }

    public OrderCustomerResponse? Customer { get; init; }

    public decimal Subtotal { get; init; }

    public decimal Shipping { get; init; }

    public decimal Tax { get; init; }

    public decimal Discount { get; init; }

    public decimal Total { get; init; }

    public string Status { get; init; } = string.Empty;

    public IReadOnlyList<OrderItemResponse> Items { get; init; } = Array.Empty<OrderItemResponse>();
}

public class OrderCustomerResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}

public class OrderItemResponse
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal Total { get; init; }
}
