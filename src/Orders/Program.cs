using Orders.Abstractions;
using Orders.Models;
using Orders.Repositories;
using Orders.Services;

// Демонстрация: создаём заказ и считаем сумму к оплате.
var repo = new InMemoryOrderRepository();
var service = new OrderService(repo, new ConsolePublisher(), new DiscountCalculator());

var cmd = new NewOrder(new List<OrderItem>
{
    new("Книга", 300m, 2),
    new("Ручка", 50m, 1),
    new("Тетрадь", 350m, 1),
});

var id = await service.CreateAsync(cmd);
var order = await repo.FindAsync(id);

Console.WriteLine($"Заказ {id}");
Console.WriteLine($"Сумма заказа: {order!.Total()}");
Console.WriteLine($"К оплате:     {await service.GetPayableAsync(id)}");

// Вместо RabbitMQ просто печатаем событие в консоль.
class ConsolePublisher : IMessagePublisher
{
    public Task PublishAsync<T>(T message)
    {
        Console.WriteLine($"[событие] {typeof(T).Name}: {message}");
        return Task.CompletedTask;
    }
}
