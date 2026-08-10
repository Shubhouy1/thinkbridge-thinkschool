namespace LegacyOrders.Services;

public class DefaultDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(decimal subtotal, string couponCode, bool isVip)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            return 0m;
        }

        return couponCode switch
        {
            "SAVE10" => subtotal * 0.10m,
            "SAVE20" => subtotal * 0.20m,
            "VIP" when isVip => subtotal * 0.15m,
            _ => 0m
        };
    }
}
