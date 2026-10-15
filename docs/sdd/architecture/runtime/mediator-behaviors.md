# Наскрізні behaviors Mediator

Host реєструє ці behaviors для всіх запитів Mediator саме в такому порядку:

~~~text
ISender.Send(...)
  │
  ├── 1. RequestLoggingBehavior
  ├── 2. ExceptionBehavior
  ├── 3. PerformanceBehavior
  ├── 4. ValidationBehavior
  ├── 5. DomainEventDispatcherBehavior
  └── Handler
~~~

## 1. RequestLoggingBehavior

Логує початок, завершення й контекст збою запиту Mediator за чинними правилами структурованих логів. Це лише спостережуваність: behavior не змінює бізнес-поведінку й не логує чутливий payload запиту.

## 2. ExceptionBehavior

Забезпечує чинний шлях обробки винятків навколо запиту. Очікувані бізнес-результати й надалі представляють через Ardalis.Result, а не винятками.

## 3. PerformanceBehavior

Вимірює тривалість запиту за налаштованими параметрами PerformanceBehavior і записує діагностику повільних запитів. Не змінює семантику query/command.

## 4. ValidationBehavior

Виконує всі валідатори FluentValidation для запиту перед handler. Для відповідей `Result` або `Result<T>` помилки стають `Result.Invalid`, а handler не викликається. Валідатори перевіряють вхідні контракти, а не правила стану aggregate.

## 5. DomainEventDispatcherBehavior

Виконується після успішного handler. Отримує накопичені події з `IDomainEventContext`, очищає їх і публікує кожну через `IDomainEventDispatcher`/Mediator. Сутності накопичують події; controllers і handlers не обходять цей цикл.

## Правила для нового behavior

- Додавай behavior лише для справді наскрізної потреби кількох use cases.
- Свідомо зберігай порядок; зміну порядку документуй в [ADR](../decisions/README.md) та зачепленій специфікації фічі.
- Не розміщуй у поведінці конвеєра доменне правило, авторизацію окремої кінцевої точки або координацію сценарію використання.
