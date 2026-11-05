namespace Orders.Models;

/// <summary>Событие «заказ создан», публикуется в брокер сообщений.</summary>
public record OrderCreated(Guid OrderId);
