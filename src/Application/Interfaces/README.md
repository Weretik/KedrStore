# Интерфейсы приложения

## Обзор

Этот каталог содержит интерфейсы, которые определяют контракты между слоем приложения и внешним миром (инфраструктура, пользовательский интерфейс). Эти интерфейсы реализуют принцип инверсии зависимостей (Dependency Inversion Principle) из SOLID, позволяя слою приложения быть независимым от конкретных реализаций.

## Интерфейсы

### ICurrentUserService

Интерфейс для получения информации о текущем пользователе. Используется для доступа к идентификатору пользователя, его ролям и разрешениям в бизнес-логике приложения.

### IDateTimeProvider

Абстракция для получения текущей даты и времени. Улучшает тестируемость кода, позволяя подменять время в тестах.

### IApplicationDbContext

Интерфейс для работы с базой данных приложения. Определяет набор сущностей и методы для взаимодействия с ними.

### IUnitOfWork

Реализация паттерна Unit of Work для атомарного сохранения изменений и управления транзакциями.

### ICacheService

Интерфейс для работы с кэшем. Позволяет абстрагироваться от конкретной реализации кэширования.

### IEmailService

Сервис для отправки электронных писем. Поддерживает шаблоны и вложения.

### IFileStorageService

Интерфейс для работы с файловым хранилищем. Позволяет сохранять, получать и удалять файлы.

### IBackgroundJobService

Сервис для работы с фоновыми задачами. Поддерживает отложенное и периодическое выполнение задач.

## Использование

Эти интерфейсы должны быть реализованы в слое инфраструктуры и зарегистрированы в DI-контейнере. Слой приложения использует эти интерфейсы через внедрение зависимостей, не завися от конкретных реализаций.

```csharp
// Пример использования в обработчике команды
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;

    public CreateUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Использование сервисов через интерфейсы
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            CreatedBy = _currentUserService.UserId,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Отправка письма
        await _emailService.SendTemplateAsync(
            "UserWelcome",
            new { user.Email, user.UserName },
            user.Email,
            "Добро пожаловать!",
            cancellationToken);

        return Result.Success(user.Id);
    }
}
```

## Реализация

Реализации этих интерфейсов находятся в проекте Infrastructure и регистрируются в DI-контейнере через методы расширения `AddInfrastructureServices()`.
