using Orders.Abstractions;
using Orders.Models;

namespace Orders.Services;

/// <summary>Создание заказов и расчёт суммы к оплате.</summary>
public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly IMessagePublisher _publisher;
    private readonly DiscountCalculator _discounts;

    public OrderService(IOrderRepository repo, IMessagePublisher publisher, DiscountCalculator discounts)
    {
        _repo = repo;
        _publisher = publisher;
        _discounts = discounts;
    }

    /// <summary>Сохраняет заказ и публикует событие OrderCreated. Возвращает Id заказа.</summary>
    public async Task<Guid> CreateAsync(NewOrder cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (cmd.Items.Count == 0)
            throw new ArgumentException("Заказ не может быть пустым", nameof(cmd));

        var order = new Order(cmd.Items);

        await _repo.SaveAsync(order);

        await _publisher.PublishAsync(new OrderCreated(order.Id));

        return order.Id;
    }

    /// <summary>Сумма к оплате: сумма заказа минус скидка.</summary>
    public async Task<decimal> GetPayableAsync(Guid orderId)
    {
        var order = await _repo.FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Заказ {orderId} не найден");

        var total = order.Total();

        return total - _discounts.Calculate(total);
    }
}
