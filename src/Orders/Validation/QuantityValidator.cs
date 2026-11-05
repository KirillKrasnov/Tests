namespace Orders.Validation;

/// <summary>Проверка количества товара в позиции заказа.</summary>
public static class QuantityValidator
{
    public const int Min = 1;
    public const int Max = 100;

    public static bool IsValidQty(int qty) => qty >= Min && qty <= Max;
}
