# CQRS паттерн в KedrStore

## Обзор

В проекте KedrStore используется паттерн CQRS (Command Query Responsibility Segregation) для разделения операций чтения и изменения данных. Этот подход обеспечивает следующие преимущества:

- Четкое разделение ответственности между компонентами системы
- Упрощение модели предметной области для конкретных сценариев использования
- Возможность оптимизации операций чтения и записи независимо друг от друга
- Улучшение тестируемости и поддерживаемости кода

## Основные компоненты

### Команды (Commands)

Команды представляют намерение изменить состояние системы. В KedrStore они реализуют интерфейс `ICommand<TResult>` или `ICommand` для команд без возвращаемого значения.

Пример команды:
```csharp
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

### Запросы (Queries)

Запросы служат для получения данных без изменения состояния системы. Они реализуют интерфейс `IQuery<TResult>`.

Пример запроса:
```csharp
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}
```

### Обработчики (Handlers)

Каждая команда или запрос имеет соответствующий обработчик, который реализует бизнес-логику:

- `ICommandHandler<TCommand, TResult>` - для обработки команд
- `IQueryHandler<TQuery, TResult>` - для обработки запросов

Пример обработчика команды:
```csharp
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(command.Name, command.Price, command.Description);
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
```

### Пагинация

Для поддержки пагинации в запросах используются интерфейсы:

- `IHasPagination` - для запросов, требующих пагинацию
- `IPagedList<T>` - для возврата пагинированных результатов

### Валидация

Валидация входных данных осуществляется с помощью интерфейса `IValidator<T>`, который проверяет корректность команд и запросов перед их обработкой.

## Практические рекомендации

1. Команды должны представлять одну конкретную операцию
2. Запросы должны возвращать только необходимые данные (DTO)
3. Обработчики не должны вызывать друг друга напрямую
4. Используйте валидацию для проверки входных данных перед обработкой
5. Команды и запросы должны быть неизменяемыми (immutable) после создания

## Пример полного потока

1. Контроллер или другой компонент создает команду/запрос
2. Медиатор направляет команду/запрос соответствующему обработчику
3. Валидатор проверяет входные данные
4. Обработчик выполняет бизнес-логику
5. Результат возвращается вызывающей стороне

Этот подход обеспечивает четкое разделение ответственности и упрощает поддержку системы при росте её сложности.
