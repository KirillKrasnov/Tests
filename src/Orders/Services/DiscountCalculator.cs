namespace Orders.Services;

/// <summary>Расчёт скидки на заказ.</summary>
public class DiscountCalculator
{
    public const decimal Threshold = 1000m;
    public const decimal Rate = 0.10m;

    public decimal Calculate(decimal total)
    {
        if (total < 0)
            throw new ArgumentOutOfRangeException(nameof(total), "Сумма не может быть отрицательной");

        // Правило: скидка 10% для заказов от 1000 включительно
        if (total > Threshold)
            return total * Rate;

        return 0;
    }
}
