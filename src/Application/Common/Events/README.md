﻿# Система доменных событий

## Обзор

Система доменных событий позволяет реализовать слабо связанную архитектуру, где различные части приложения могут реагировать на события, происходящие в домене, без прямой зависимости друг от друга.

## Основные компоненты

1. **IDomainEvent** (`Application.Common.Abstractions.Events`) - базовый интерфейс для всех доменных событий
2. **DomainEvent** (`Application.Common.Events`) - абстрактный базовый класс для доменных событий
3. **IDomainEventHandler<T>** (`Application.Common.Abstractions.Events`) - интерфейс для обработчиков событий
4. **IDomainEventDispatcher** (`Application.Common.Abstractions.Events`) - интерфейс для диспетчера событий
5. **IHasDomainEvents** (`Application.Common.Abstractions.Events`) - интерфейс для сущностей с доменными событиями

## Использование

Для использования системы доменных событий необходимо:

1. Создать классы конкретных доменных событий, унаследованные от `DomainEvent`
2. Создать обработчики событий, реализующие интерфейс `IDomainEventHandler<T>`
3. Реализовать интерфейс `IHasDomainEvents` в доменных сущностях
4. Вызывать метод `AddDomainEvent()` в доменных сущностях для генерации событий
5. Зарегистрировать обработчики событий через методы расширения в `Application.DependencyInjection`

## Регистрация компонентов

В файле `Program.cs` или конфигурации сервисов:

```csharp
// Импорт пространства имен для методов расширения
using Application.DependencyInjection;

// Добавление всех сервисов приложения, включая MediatR и поведения
services.AddApplicationServices();

// Или более детальная регистрация:

// Регистрация всех обработчиков из сборки
services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());

// Регистрация конкретного обработчика
services.AddDomainEventHandler<SomeEvent, SomeEventHandler>();

// Регистрация диспетчера событий (должна быть реализована в инфраструктурном слое)
services.AddScoped<IDomainEventDispatcher, YourDomainEventDispatcher>();
```

## Преимущества использования доменных событий

1. **Слабая связанность** - компоненты не зависят напрямую друг от друга
2. **Разделение ответственности** - каждый обработчик выполняет одну конкретную задачу
3. **Масштабируемость** - легко добавлять новые обработчики без изменения существующего кода
4. **Тестируемость** - обработчики можно тестировать изолированно
5. **Соответствие бизнес-терминологии** - события названы в соответствии с бизнес-процессами
