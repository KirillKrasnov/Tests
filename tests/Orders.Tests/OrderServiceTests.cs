using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orders.Abstractions;
using Orders.Models;
using Orders.Services;

namespace Orders.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IMessagePublisher> _pubMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        // могаем 
        _sut = new OrderService(_repoMock.Object, _pubMock.Object, new DiscountCalculator());
    }

    private static NewOrder CreateValidCommand() =>
        new(new List<OrderItem> { new("Книга", 300m, 2) });

    // Тест 10 инвавалидный заказ сохраняется в репозиторий ровно 1 раз
    [Fact]
    public async Task CreateAsync_ValidOrder_SavesOrder()
    {
        var cmd = CreateValidCommand();

        await _sut.CreateAsync(cmd);

        _repoMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
    }

    // Тест 11 публикуется событие создания заказа с тем же ID
    [Fact]
    public async Task CreateAsync_ValidOrder_PublishesEventWithSameId()
    {
        var cmd = CreateValidCommand();

        var createdId = await _sut.CreateAsync(cmd);

        _pubMock.Verify(p => p.PublishAsync(It.Is<OrderCreated>(e => e.OrderId == createdId)), Times.Once);
    }

    // Тест 12 если сохранение в репозиторий упало, событие в брокер НЕ публикуется
    [Fact]
    public async Task CreateAsync_SaveFails_DoesNotPublish()
    {
        var cmd = CreateValidCommand();
        _repoMock.Setup(r => r.SaveAsync(It.IsAny<Order>())).ThrowsAsync(new Exception("DB Error"));

        await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(cmd));

        _pubMock.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Never);
    }

    // Тест 13: null вместо команды вызовет ArgumentNullException
    [Fact]
    public async Task CreateAsync_NullCommand_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
    }

    // Тест 14 пустой список товаров выдает ArgumentException и не сохраняет данные
    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsAndDoesNotSave()
    {
        var emptyCmd = new NewOrder(new List<OrderItem>());

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(emptyCmd));
        _repoMock.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never);
    }

    // Тест 15 исключение при публикации события должно пробрасываться выше
    [Fact]
    public async Task CreateAsync_PublishFails_ExceptionPropagates()
    {
        var cmd = CreateValidCommand();
        _pubMock.Setup(p => p.PublishAsync(It.IsAny<OrderCreated>())).ThrowsAsync(new TimeoutException());

        await Assert.ThrowsAsync<TimeoutException>(() => _sut.CreateAsync(cmd));
    }

    // Тест 16 сумма к оплате без скидки 
    [Fact]
    public async Task GetPayableAsync_TotalBelowThreshold_NoDiscount()
    {
        var orderId = Guid.NewGuid();
        var order = new Order();
        order.Add("Товар", 500m, 1); // Сумма 500 

        _repoMock.Setup(r => r.FindAsync(orderId)).ReturnsAsync(order);

        var payable = await _sut.GetPayableAsync(orderId);

        Assert.Equal(500m, payable);
    }

    // Тест 17 сумма к оплате со скидкой 
    [Fact]
    public async Task GetPayableAsync_TotalAboveThreshold_AppliesDiscount()
    {
        var orderId = Guid.NewGuid();
        var order = new Order();
        order.Add("Товар", 1500m, 1); 

        _repoMock.Setup(r => r.FindAsync(orderId)).ReturnsAsync(order);

        var payable = await _sut.GetPayableAsync(orderId);

        Assert.Equal(1350m, payable);
    }

    // Тест 18 если заказ не найден, выбрасывается KeyNotFoundException
    [Fact]
    public async Task GetPayableAsync_OrderNotFound_Throws()
    {
        var orderId = Guid.NewGuid();
        _repoMock.Setup(r => r.FindAsync(orderId)).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetPayableAsync(orderId));
    }
}
