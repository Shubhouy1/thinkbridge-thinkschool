namespace LegacyOrders.Services;

public interface IDiscountStrategy
{
    decimal Calculate(decimal subtotal, string couponCode, bool isVip);
}
