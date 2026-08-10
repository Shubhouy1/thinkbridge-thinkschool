namespace LegacyOrders.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal Shipping { get; set; }

    public decimal Tax { get; set; }

    public decimal Discount { get; set; }

    public decimal Total { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}