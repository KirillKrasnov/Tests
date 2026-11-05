using Xunit;
using Orders.Services;
using System;

namespace Orders.Tests;

public class DiscountCalculatorTests
{
    // Тест 8 расчет ожидаемой скидки для разных сумм
    [Theory]
    [InlineData(0, 0)]
    [InlineData(999.99, 0)]
    [InlineData(1000, 0)]  
    [InlineData(2000, 200)]
    public void Calculate_ReturnsExpected(decimal total, decimal expectedDiscount)
    {
        var calculator = new DiscountCalculator();
        var discount = calculator.Calculate(total);
        Assert.Equal(expectedDiscount, discount);
    }

    // Тест 9 отрицательная сумма заказа вызывает ошибку
    [Fact]
    public void Calculate_NegativeTotal_Throws()
    {
        var calculator = new DiscountCalculator();
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(-500m));
    }
}
