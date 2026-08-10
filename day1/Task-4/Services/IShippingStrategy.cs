namespace LegacyOrders.Services;

public interface IShippingStrategy
{
    decimal Calculate(decimal subtotal, string country);
}
