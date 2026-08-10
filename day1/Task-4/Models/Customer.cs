namespace LegacyOrders.Models;

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public bool IsVip { get; set; }

    public DateTime? LastOrderDate { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalSpent { get; set; }

    public int VipPoints { get; set; }
}