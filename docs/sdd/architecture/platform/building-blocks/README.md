# Довідник BuildingBlocks

BuildingBlocks — спільна технічна основа. Вона визначає міжмодульні примітиви та реалізації, але не володіє бізнес-правилами Catalog, Sales чи Identity.

## Карта проєктів

~~~text
src/BuildingBlocks/
├── BuildingBlocks.Domain/            примітиви сутностей, аудиту, помилок і подій
├── BuildingBlocks.Application/       Mediator behaviors, контракти, notifications і логи
├── BuildingBlocks.Infrastructure/    спільні адаптери, міграції й сидери
├── BuildingBlocks.Api/               MVC-відображення Ardalis.Result
└── BuildingBlocks.Integrations.OneC/ спільний SOAP client factory та автентифікація
~~~

## Порядок читання

1. [Примітиви Domain](domain.md)
2. [Примітиви Application і behaviors](application.md)
3. [Сервіси Infrastructure та запуск](infrastructure.md)
4. [Відображення результату API](api.md)
5. [Основа інтеграції з 1C](one-c.md)
6. [Правила використання](usage-rules.md)

Перед створенням нового спільного базового типу, behavior або сервісної абстракції звір цей довідник.
