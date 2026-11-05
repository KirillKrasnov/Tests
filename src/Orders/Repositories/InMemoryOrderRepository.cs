using Orders.Abstractions;
using Orders.Models;

namespace Orders.Repositories;

/// <summary>Хранилище заказов в памяти (fake-реализация вместо базы данных).</summary>
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _orders = new();

    public Task SaveAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _orders[order.Id] = order;

        return Task.CompletedTask;
    }

    public Task<Order?> FindAsync(Guid id)
    {
        _orders.TryGetValue(id, out var order);

        return Task.FromResult<Order?>(order);
    }
}
