namespace LegacyOrders.Models;

public class OrderRequest
{
    public int CustomerId { get; set; }

    public string Country { get; set; } = string.Empty;

    public string CouponCode { get; set; } = string.Empty;

    public bool SendEmail { get; set; }

    public List<OrderRequestItem> Items { get; set; } = [];
}

public class OrderRequestItem
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}