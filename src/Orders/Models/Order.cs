using Orders.Validation;

namespace Orders.Models;

/// <summary>Заказ: набор позиций и расчёт общей стоимости.</summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Order()
    {
    }

    public Order(IEnumerable<OrderItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            Add(item.Name, item.Price, item.Quantity);
        }
    }

    public Guid Id { get; } = Guid.NewGuid();

    public IReadOnlyList<OrderItem> Items => _items;

    public void Add(string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название товара не может быть пустым", nameof(name));

        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Цена должна быть больше нуля");

        if (!QuantityValidator.IsValidQty(quantity))
            throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть от 1 до 100");

        _items.Add(new OrderItem(name, price, quantity));
    }

    /// <summary>Сумма заказа без скидки.</summary>
    public decimal Total() => _items.Sum(i => i.Price * i.Quantity);
}
