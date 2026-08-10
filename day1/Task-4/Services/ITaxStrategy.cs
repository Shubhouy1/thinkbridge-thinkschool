namespace LegacyOrders.Services;

public interface ITaxStrategy
{
    decimal Calculate(decimal subtotal, string state);
}
