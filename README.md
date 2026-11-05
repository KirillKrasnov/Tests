# Практика: unit-тесты на C# (xUnit и Moq)

Учебное приложение «Заказы». В нём несколько классов, и ваша задача — покрыть их unit-тестами.
Один тест уже написан (`OrderTests.Total_TwoItems_ReturnsSum`), остальные пишете вы по его образцу.

## 1. Что нужно установить

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022, JetBrains Rider или VS Code с расширением C# Dev Kit (по желанию)
- Аккаунт на [GitHub](https://github.com) и Git (для разделов 9–10)

Проверьте, что SDK установлен:

```bash
dotnet --version     # должно вывести 8.x.x
```

## 2. Структура проекта

```text
OrdersPractice/
├── OrdersPractice.sln
├── src/Orders/                       # тестируемый код
│   ├── Models/
│   │   ├── Order.cs                  # заказ: позиции, Add, Total
│   │   ├── OrderItem.cs
│   │   ├── NewOrder.cs
│   │   └── OrderCreated.cs           # событие для брокера
│   ├── Validation/QuantityValidator.cs
│   ├── Services/
│   │   ├── DiscountCalculator.cs     # расчёт скидки
│   │   └── OrderService.cs           # создание заказа, сумма к оплате
│   ├── Handlers/OrderCreatedHandler.cs   # обработчик сообщения
│   ├── Repositories/InMemoryOrderRepository.cs
│   ├── Abstractions/Interfaces.cs    # IOrderRepository, IMessagePublisher, INotificationSender
│   └── Program.cs                    # демо-запуск
└── tests/Orders.Tests/               # сюда пишете тесты
    ├── Orders.Tests.csproj           # xUnit + Moq + coverlet уже подключены
    └── OrderTests.cs                 # один готовый тест
```

## 3. Как запустить

Все команды выполняются из папки `OrdersPractice` (где лежит `OrdersPractice.sln`).

```bash
dotnet restore      # скачать пакеты (нужен интернет)
dotnet build        # собрать проект
dotnet test         # запустить все тесты
```

Для готового теста в конце должно быть примерно так:

```text
Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1
```

Полезные варианты запуска:

```bash
# только один класс тестов
dotnet test --filter "FullyQualifiedName~OrderTests"

# только один тест по имени
dotnet test --filter "Name=Total_TwoItems_ReturnsSum"

# подробный вывод (видно названия тестов)
dotnet test --logger "console;verbosity=detailed"

# автоперезапуск тестов при сохранении файла
dotnet watch test --project tests/Orders.Tests
```

В Visual Studio и Rider тесты видны в окне **Test Explorer** (меню Test → Test Explorer), оттуда их можно запускать по одному.

Демонстрационный запуск самого приложения (без тестов):

```bash
dotnet run --project src/Orders
```

## 4. Готовый тест — разберите его

Файл `tests/Orders.Tests/OrderTests.cs`:

```csharp
[Fact]
public void Total_TwoItems_ReturnsSum()
{
    // Arrange
    var order = new Order();
    order.Add("Книга", 300m, 2);
    order.Add("Ручка", 50m, 1);

    // Act
    var total = order.Total();

    // Assert
    Assert.Equal(650m, total);
}
```

Обратите внимание: структура Arrange–Act–Assert, имя по схеме `Метод_Условие_Результат`, одна проверка одного поведения.

## 5. Какие тесты нужно написать

Создавайте по одному файлу на класс: `DiscountCalculatorTests.cs`, `OrderServiceTests.cs` и т.д.
Уровень: **Б** — базовый, **С** — средний, **П** — продвинутый.

### Order (`OrderTests.cs`, файл уже есть, добавьте тесты)

| № | Тест | Что проверить | Тип | Ур. |
|---|------|---------------|-----|-----|
| 1 | `Total_EmptyOrder_ReturnsZero` | У пустого заказа сумма 0 | Fact | Б |
| 2 | `Add_InvalidName_Throws` | Названия `null`, `""`, `"   "` дают `ArgumentException` | Theory | Б |
| 3 | `Add_NonPositivePrice_Throws` | Цены `0` и `-1` дают `ArgumentOutOfRangeException` | Theory | Б |
| 4 | `Add_InvalidQuantity_Throws` | Количества `0`, `-1`, `101` дают `ArgumentOutOfRangeException` | Theory | Б |
| 5 | `Add_ValidItem_AddsToItems` | После `Add` в `Items` одна позиция с теми же данными | Fact | Б |
| 6 | `Constructor_WithItems_SameTotalAsAdd` | Заказ, созданный из списка позиций, имеет правильную сумму | Fact | С |

### QuantityValidator (`QuantityValidatorTests.cs`)

| № | Тест | Что проверить | Тип | Ур. |
|---|------|---------------|-----|-----|
| 7 | `IsValidQty_ChecksBounds` | Значения `-1, 0, 1, 100, 101`: допустимы только `1` и `100` | Theory | Б |

### DiscountCalculator (`DiscountCalculatorTests.cs`)

| № | Тест | Что проверить | Тип | Ур. |
|---|------|---------------|-----|-----|
| 8 | `Calculate_ReturnsExpected` | Суммы `0`, `999.99`, `1000`, `2000` дают скидки `0`, `0`, `100`, `200` (по правилу «от 1000 включительно») | Theory | Б |
| 9 | `Calculate_NegativeTotal_Throws` | Отрицательная сумма даёт `ArgumentOutOfRangeException` | Fact | Б |

### OrderService (`OrderServiceTests.cs`, нужен Moq)

| № | Тест | Что проверить | Тип | Ур. |
|---|------|---------------|-----|-----|
| 10 | `CreateAsync_ValidOrder_SavesOrder` | `SaveAsync` вызван ровно один раз | Fact (mock) | С |
| 11 | `CreateAsync_ValidOrder_PublishesEventWithSameId` | Опубликовано одно `OrderCreated`, его `OrderId` совпадает с возвращённым Id | Fact (mock) | С |
| 12 | `CreateAsync_SaveFails_DoesNotPublish` | Если `SaveAsync` бросил исключение, событие не опубликовано (`Times.Never`) | Fact (mock) | С |
| 13 | `CreateAsync_NullCommand_Throws` | `null` вместо команды даёт `ArgumentNullException` | Fact | Б |
| 14 | `CreateAsync_EmptyItems_ThrowsAndDoesNotSave` | Пустой список позиций даёт `ArgumentException`, `SaveAsync` не вызван | Fact (mock) | С |
| 15 | `CreateAsync_PublishFails_ExceptionPropagates` | Если публикация бросила исключение, оно дошло до вызывающего кода | Fact (mock) | П |
| 16 | `GetPayableAsync_TotalBelowThreshold_NoDiscount` | Заказ на 500: к оплате 500 | Fact (stub) | С |
| 17 | `GetPayableAsync_TotalAboveThreshold_AppliesDiscount` | Заказ на 1500: к оплате 1350 | Fact (stub) | С |
| 18 | `GetPayableAsync_OrderNotFound_Throws` | Репозиторий вернул `null`: `KeyNotFoundException` | Fact (stub) | С |

### OrderCreatedHandler (`OrderCreatedHandlerTests.cs`, нужен Moq)

| № | Тест | Что проверить | Тип | Ур. |
|---|------|---------------|-----|-----|
| 19 | `HandleAsync_ValidMessage_SendsNotification` | `SendAsync` вызван один раз, текст содержит Id заказа | Fact (mock) | С |
| 20 | `HandleAsync_EmptyOrderId_Throws` | `Guid.Empty` даёт `ArgumentException`, сообщение содержит `OrderId`, уведомление не отправлено | Fact (async) | С |
| 21 | `HandleAsync_NullMessage_Throws` | `null` даёт `ArgumentNullException` | Fact (async) | Б |

### InMemoryOrderRepository (`InMemoryOrderRepositoryTests.cs`, моки не нужны)

| № | Тест | Что проверить | Тип | Ур. |
|---|------|---------------|-----|-----|
| 22 | `SaveAsync_ThenFindAsync_ReturnsSameOrder` | Сохранённый заказ находится по Id | Fact (async) | Б |
| 23 | `FindAsync_UnknownId_ReturnsNull` | Для неизвестного Id возвращается `null` | Fact (async) | Б |
| 24 | `SaveAsync_Null_Throws` | `null` даёт `ArgumentNullException` | Fact (async) | Б |

**Минимум для зачёта:** 15 тестов из списка, среди них не меньше 3 `Theory` и не меньше 4 тестов с `Moq` (`Verify`).

## 6. Требования к тестам

- Структура Arrange–Act–Assert, имя по схеме `Метод_Условие_Результат`.
- Один тест проверяет одно поведение, внутри нет `if` и циклов.
- Тесты независимы друг от друга и не зависят от порядка запуска.
- Для границ используйте `[Theory]` и `[InlineData]`.
- Асинхронные тесты возвращают `Task` и используют `await` (никаких `async void` и `.Result`).
- Для исключений: `Assert.Throws<T>` и `await Assert.ThrowsAsync<T>`.
- Базу и брокер не используйте: зависимости подменяйте моками (`Moq`) или `InMemoryOrderRepository`.

## 7. Заготовка для тестов с Moq

```csharp
using Moq;
using Orders.Abstractions;
using Orders.Models;
using Orders.Services;

namespace Orders.Tests;

public class OrderServiceTests
{
    readonly Mock<IOrderRepository> _repo = new();
    readonly Mock<IMessagePublisher> _pub = new();
    readonly OrderService _sut;

    public OrderServiceTests() =>
        _sut = new(_repo.Object, _pub.Object, new DiscountCalculator());

    static NewOrder Cmd() =>
        new(new List<OrderItem> { new("Книга", 300m, 2) });

    // Примеры вызовов Moq:
    //
    // настроить ответ:
    //   _repo.Setup(r => r.FindAsync(id)).ReturnsAsync(order);
    //   _repo.Setup(r => r.FindAsync(id)).ReturnsAsync((Order?)null);
    //
    // настроить исключение:
    //   _repo.Setup(r => r.SaveAsync(It.IsAny<Order>()))
    //        .ThrowsAsync(new TimeoutException());
    //
    // проверить вызов:
    //   _pub.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Once());
    //   _pub.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Never());
    //   _pub.Verify(p => p.PublishAsync(
    //       It.Is<OrderCreated>(e => e.OrderId == id)), Times.Once());
}
```

## 8. Покрытие кода

```bash
# 1. собрать данные о покрытии
dotnet test --collect:"XPlat Code Coverage"

# 2. один раз установить генератор отчётов
dotnet tool install -g dotnet-reportgenerator-globaltool

# 3. построить HTML-отчёт
reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:coverage-report

# 4. открыть coverage-report/index.html в браузере
```

Цель: покрытие строк классов `Order`, `QuantityValidator`, `DiscountCalculator`, `OrderService`, `OrderCreatedHandler`, `InMemoryOrderRepository` не ниже 90%.
В отчёте по практике объясните, какие участки остались непокрытыми и почему.

## 9. Автоматический запуск тестов в GitHub Actions (CI)

Локальный запуск `dotnet test` полезен, но не гарантирует, что тесты пройдут у всех участников и в актуальной версии кода. **Непрерывная интеграция (CI)** решает эту проблему: при каждом `push` или `pull request` GitHub автоматически собирает проект и запускает все тесты. Это соответствует критерию «независимость от внешнего окружения» и помогает сразу увидеть, если изменение что-то сломало.

> Официальное руководство Microsoft:  
> [Краткое руководство: создание рабочего процесса валидации теста GitHub](https://learn.microsoft.com/ru-ru/dotnet/devops/dotnet-test-github-action)

### 9.1. Что нужно сделать

1. Убедитесь, что проект находится в репозитории на GitHub.
2. В корне репозитория создайте каталог `.github/workflows` (если его ещё нет).
3. В этом каталоге создайте файл, например `ci.yml`, и скопируйте в него конфигурацию из п. 9.2.

### 9.2. Пример рабочего процесса для проекта `OrdersPractice`

```yaml
name: build and test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET 8
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
```

### 9.3. Что делает этот workflow

- **`on: push / pull_request`** — workflow запускается при каждом пуше в ветку `main` и при создании пул-реквеста в неё.
- **`actions/setup-dotnet@v4`** с `dotnet-version: '8.0.x'` — устанавливает .NET SDK 8, как требуется в задании.
- **`dotnet restore`** — восстанавливает NuGet-пакеты (xUnit, Moq, coverlet).
- **`dotnet build --no-restore`** — собирает решение в конфигурации `Release` без повторного восстановления пакетов.
- **`dotnet test --no-build`** — запускает все тесты, используя уже собранные сборки.

Если хотя бы один тест упадёт, workflow завершится с ошибкой, и GitHub покажет красный крестик напротив коммита. Результаты видны во вкладке **Actions** репозитория.

### 9.4. Как проверить, что всё работает

1. Сделайте коммит и пуш в `main` (или создайте pull request).
2. Перейдите на вкладку **Actions** в GitHub.
3. Убедитесь, что workflow `build and test` запустился и все шаги зелёные.
4. Если тест падает — workflow остановится на шаге `Test`, и в логах будет видно, какой именно тест не прошёл.

### 9.5. Дополнительно (по желанию)

- Можно добавить шаг **публикации результатов тестов** (например, `dorny/test-reporter`), чтобы отчёты отображались прямо в интерфейсе pull request’а.
- Для проверки покрытия можно добавить `--collect:"XPlat Code Coverage"` в шаг `dotnet test` и сохранять артефакт с `coverage.cobertura.xml`.

CI не заменяет локальный запуск, но служит дополнительной страховкой и позволяет преподавателю видеть, что тесты проходят не только на машине студента.

## 10. Индикатор состояния сборки (status badge) в README.md

Индикатор состояния — это динамическая картинка, которая автоматически обновляется и показывает, прошли ли последние тесты в вашем workflow. Это визуальный и быстрый способ сообщить всем (и себе), что проект находится в рабочем состоянии.

### 10.1. Формат URL индикатора

Стандартный URL для индикатора состояния рабочего процесса GitHub Actions выглядит так:

```text
https://github.com/<OWNER>/<REPOSITORY>/actions/workflows/<WORKFLOW_FILE>/badge.svg
```

Где:
- **`<OWNER>`** — ваш никнейм на GitHub или название организации.
- **`<REPOSITORY>`** — название репозитория.
- **`<WORKFLOW_FILE>`** — имя файла вашего workflow (например, `ci.yml`).

### 10.2. Как добавить индикатор в README.md

Самый простой способ — использовать Markdown-разметку для вставки изображения. По умолчанию индикатор будет показывать статус последнего запуска workflow в ветке по умолчанию (обычно `main` или `master`).

**Пример для вашего workflow `ci.yml`:**

```markdown
# OrdersPractice

![CI](https://github.com/ВАШ_ЛОГИН/OrdersPractice/actions/workflows/ci.yml/badge.svg)

Учебное приложение «Заказы». Тесты на C# (xUnit и Moq)...
```

**Рекомендуемый вариант — индикатор-ссылка:**  
Чтобы при клике на индикатор пользователь попадал на страницу с результатами workflow, оберните изображение в ссылку:

```markdown
[![CI](https://github.com/ВАШ_ЛОГИН/OrdersPractice/actions/workflows/ci.yml/badge.svg)](https://github.com/ВАШ_ЛОГИН/OrdersPractice/actions/workflows/ci.yml)
```

### 10.3. Указание конкретной ветки

Если вы хотите показывать статус для ветки, отличной от основной, добавьте параметр `?branch=` в конец URL:

```markdown
![CI for develop](https://github.com/ВАШ_ЛОГИН/OrdersPractice/actions/workflows/ci.yml/badge.svg?branch=develop)
```

### 10.4. Альтернативный способ: копирование через интерфейс GitHub

GitHub позволяет сгенерировать готовый Markdown-код без ручного составления URL:

1. Перейдите на вкладку **Actions** в вашем репозитории.
2. Выберите нужный workflow (например, `build and test`) в левой панели.
3. Нажмите на кнопку с тремя точками (`...`) справа от поля «Filter workflow runs».
4. Выберите **Create status badge**.
5. При необходимости выберите ветку или событие и нажмите **Copy status badge Markdown**.
6. Вставьте скопированный код в ваш `README.md`.

### 10.5. Что делать, если индикатор не отображается

- **Серый цвет:** Workflow ещё не запускался в ветке, статус которой отображается. Индикатор появится после первого запуска.
- **Приватный репозиторий:** Индикаторы в приватных репозиториях не видны снаружи, поэтому вставить их в README, который просматривается извне, не получится.
- **Кэш браузера:** Иногда индикатор может «застрять». Попробуйте обновить страницу с очисткой кэша (Ctrl+F5).

Добавление индикатора — это финальный штрих, который делает ваш проект визуально завершённым и профессиональным.

## 11. Что сдавать

1. Папка `tests/Orders.Tests` с вашими тестами (или весь проект архивом без `bin` и `obj`).
2. Скриншот или текст вывода `dotnet test` (все тесты зелёные).
3. Скриншот отчёта покрытия.
4. Короткая записка (5–10 строк): какие ошибки в коде нашли тесты и что вы исправили.
5. **Ссылка на репозиторий GitHub** с настроенным workflow `.github/workflows/ci.yml`.
6. **Скриншот вкладки Actions** с успешно завершённым запуском workflow (все шаги зелёные).
7. **Скриншот README.md**, где отображается индикатор состояния сборки (badge).

## 12. Критерии оценки

- осмысленность сценариев (границы, позитивные и негативные случаи);
- корректное использование моков и `Verify`;
- читаемость тестов (имена, структура AAA);
- независимость от внешнего окружения (никакой базы и брокера);
- **корректно настроенный CI-workflow в GitHub Actions, который собирает проект и запускает тесты при push и pull request**;
- **наличие индикатора состояния сборки в README.md**;
- **осмысленная записка о найденных и исправленных ошибках**.

Подсказка: если тест из таблицы падает, сначала проверьте, правильно ли вы написали тест, затем перечитайте условие в комментариях к коду. Тест может показывать настоящую ошибку в программе.