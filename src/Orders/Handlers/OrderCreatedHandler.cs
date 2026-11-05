using Orders.Abstractions;
using Orders.Models;

namespace Orders.Handlers;

/// <summary>Обработчик сообщения OrderCreated (в реальной системе это consumer RabbitMQ).</summary>
public class OrderCreatedHandler
{
    private readonly INotificationSender _sender;

    public OrderCreatedHandler(INotificationSender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(OrderCreated message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.OrderId == Guid.Empty)
            throw new ArgumentException("OrderId не может быть пустым", nameof(message));

        await _sender.SendAsync($"Заказ {message.OrderId} создан");
    }
}
