using Xunit;
using Orders.Models;
using System;
using System.Collections.Generic;

namespace Orders.Tests;

public class OrderTests
{
    [Fact]
    public void Total_TwoItems_ReturnsSum()
    {
        var order = new Order();
        order.Add("Книга", 300m, 2);
        order.Add("Ручка", 50m, 1);

        var total = order.Total();

        Assert.Equal(650m, total);
    }

    // Тест 1 у пустого заказа сумма 0
    [Fact]
    public void Total_EmptyOrder_ReturnsZero()
    {
        var order = new Order();
        var total = order.Total();
        Assert.Equal(0m, total);
    }

    // Тест 2 невалидные имена товара выбрасывают ArgumentException
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Add_InvalidName_Throws(string? invalidName)
    {
        var order = new Order();
        Assert.Throws<ArgumentException>(() => order.Add(invalidName, 100m, 1));
    }

    // Тест 3 некорректная цена (0 и отрицательная)
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Add_NonPositivePrice_Throws(decimal invalidPrice)
    {
        var order = new Order();
        Assert.Throws<ArgumentOutOfRangeException>(() => order.Add("Товар", invalidPrice, 1));
    }

    // Тест 4 некорректное количество (меньше 1 или больше 100)
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void Add_InvalidQuantity_Throws(int invalidQuantity)
    {
        var order = new Order();
        Assert.Throws<ArgumentOutOfRangeException>(() => order.Add("Товар", 100m, invalidQuantity));
    }

    // Тест 5 валидный товар успешно добавляется в список Items
    [Fact]
    public void Add_ValidItem_AddsToItems()
    {
        var order = new Order();
        order.Add("Мышка", 1500m, 1);

        Assert.Single(order.Items); // в списке ровно 1 элемент
        var item = order.Items[0];
        Assert.Equal("Мышка", item.Name);
        Assert.Equal(1500m, item.Price);
        Assert.Equal(1, item.Quantity);
    }

    // Тест 6 конструктор со списком позиций считает сумму так же, как последовательный Add
    [Fact]
    public void Constructor_WithItems_SameTotalAsAdd()
    {
        var items = new List<OrderItem>
        {
            new("Товар 1", 200m, 2),
            new("Товар 2", 500m, 1)
        };

        // Создаем конструкто
        var orderFromConstructor = new Order(items);

        var orderFromAdd = new Order();
        orderFromAdd.Add("Товар 1", 200m, 2);
        orderFromAdd.Add("Товар 2", 500m, 1);

        Assert.Equal(orderFromAdd.Total(), orderFromConstructor.Total());
    }
}
