namespace Orders.Models;

/// <summary>Позиция заказа: товар, цена за штуку и количество.</summary>
public record OrderItem(string Name, decimal Price, int Quantity);
