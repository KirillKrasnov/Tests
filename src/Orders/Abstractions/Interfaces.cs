using Orders.Models;

namespace Orders.Abstractions;

/// <summary>Хранилище заказов (в реальной системе это база данных).</summary>
public interface IOrderRepository
{
    Task SaveAsync(Order order);

    Task<Order?> FindAsync(Guid id);
}

/// <summary>Публикация сообщений в брокер (в реальной системе это RabbitMQ).</summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(T message);
}

/// <summary>Отправка уведомлений (письмо, push и т.п.).</summary>
public interface INotificationSender
{
    Task SendAsync(string text);
}
