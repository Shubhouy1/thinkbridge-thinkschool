namespace LegacyOrders.Services;

public class DefaultTaxStrategy : ITaxStrategy
{
    public decimal Calculate(decimal subtotal, string state)
    {
        return state switch
        {
            "CA" => subtotal * 0.0725m,
            "NY" => subtotal * 0.08m,
            _ => subtotal * 0.05m
        };
    }
}
