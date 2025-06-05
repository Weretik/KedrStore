# Руководство по слою Application

## Введение

Слой Application (Приложение) является ключевым слоем в архитектуре KedrStore, который содержит логику конкретных сценариев использования (use cases) системы. Этот слой выступает посредником между внешними слоями (Presentation, Infrastructure) и внутренним слоем Domain.

## Структура слоя

```
Application/
├── Common/
│   ├── Abstractions/         # Базовые интерфейсы и абстракции
│   ├── Behaviors/            # Сквозная логика (cross-cutting concerns)
│   ├── Exceptions/           # Исключения уровня приложения
│   └── Extensions/           # Расширения
├── Features/                 # Бизнес-функциональность по модулям
│   ├── Products/
│   │   ├── Commands/         # Команды для изменения состояния
│   │   │   ├── Create/
│   │   │   ├── Update/
│   │   │   └── Delete/
│   │   └── Queries/          # Запросы для получения данных
│   │       ├── GetById/
│   │       ├── GetList/
│   │       └── GetPaged/
│   ├── Orders/
│   └── Users/
├── Interfaces/               # Интерфейсы для внешних сервисов
│   ├── Persistence/          # Интерфейсы для работы с хранилищем данных
│   └── Infrastructure/       # Интерфейсы для внешних сервисов
└── Services/                 # Сервисы, реализующие бизнес-логику
```

## Основные абстракции

В проекте используется паттерн CQRS (Command Query Responsibility Segregation), который разделяет операции чтения и записи. Основные абстракции:

### IUseCase

Базовый маркерный интерфейс для всех случаев использования.

```csharp
public interface IUseCase
{
}
```

### ICommand и IQuery

Интерфейсы для команд (изменяющих состояние) и запросов (только чтение).

```csharp
public interface ICommand<out TResult> : IUseCase
{
}

public interface IQuery<out TResult> : IUseCase
{
}
```

### ICommandHandler и IQueryHandler

Обработчики для команд и запросов.

```csharp
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

### Пагинация

Для работы с пагинацией используются интерфейсы `IHasPagination` и `IPagedList<T>`.

## Создание новой функциональности

### Команда (Command)

1. Создайте класс команды, реализующий `ICommand<TResult>`
2. Создайте обработчик, реализующий `ICommandHandler<TCommand, TResult>`
3. При необходимости добавьте валидатор, реализующий `IValidator<TCommand>`

Пример:

```csharp
// Команда
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Обработчик
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(command.Name, command.Price);
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
```

### Запрос (Query)

1. Создайте класс запроса, реализующий `IQuery<TResult>`
2. Создайте обработчик, реализующий `IQueryHandler<TQuery, TResult>`

Пример:

```csharp
// Запрос
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}

// Обработчик
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Продукт с ID {query.Id} не найден");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}
```

## Рекомендации

1. Придерживайтесь принципа единой ответственности (SRP) — каждый обработчик должен выполнять одну конкретную задачу
2. Используйте DTO для передачи данных между слоями
3. Не включайте инфраструктурные детали в слой Application
4. Обработчики должны быть независимыми и не вызывать друг друга напрямую
5. Используйте валидацию для проверки входных данных
6. Для сложных операций с множественными шагами используйте паттерн Unit of Work
