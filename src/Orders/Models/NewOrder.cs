namespace Orders.Models;

/// <summary>Команда на создание заказа.</summary>
public record NewOrder(IReadOnlyList<OrderItem> Items);
