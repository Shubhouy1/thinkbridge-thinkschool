namespace LegacyOrders.Services;

public class DefaultShippingStrategy : IShippingStrategy
{
    public decimal Calculate(decimal subtotal, string country)
    {
        decimal shipping = subtotal < 50m ? 9.99m : subtotal < 100m ? 4.99m : 0m;
        if (string.Equals(country, "US", StringComparison.OrdinalIgnoreCase))
        {
            shipping *= 0.5m;
        }

        return shipping;
    }
}
