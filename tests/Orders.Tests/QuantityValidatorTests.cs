using Xunit;
using Orders.Validation;

namespace Orders.Tests;

public class QuantityValidatorTests
{
    // тест 7 проверка границ допустимого количества (от 1 до 100)
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void IsValidQty_ChecksBounds(int quantity, bool expectedResult)
    {
        // Вызываем метод напрямую у статического класса без создания объекта через new
        var result = QuantityValidator.IsValidQty(quantity);

        Assert.Equal(expectedResult, result);
    }
}
