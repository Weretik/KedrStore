# Структура Application

## Відповідальність

Application виконує один сценарій використання за раз. Вона координує Domain і абстракції, але не володіє реалізацією збереження даних або транспортного рівня.

## Цільова структура фічі

~~~text
<Module>.Application/
├── Features/
│   └── <Area>/
│       └── <UseCase>/
│           ├── <UseCase>Command.cs | <UseCase>Query.cs
│           ├── <UseCase>CommandHandler.cs | <UseCase>QueryHandler.cs
│           ├── DTOs/               приватні DTO сценарію, лише за потреби
│           ├── Validators/
│           │   └── <UseCase>Validator.cs
│           └── Extensions/         лише пов'язані LINQ/mapping helpers
├── Contracts/
│   ├── Persistence/                абстракції DbContext/read/repository
│   ├── Integrations/               абстракції зовнішніх сервісів
│   └── Services/
├── Integrations/                   координація зовнішніх потоків
├── Jobs/                           логіка завдання для сценарію, не підключення планувальника
└── <Module>ApplicationAssemblyMarker.cs
~~~

## Схема сценарію використання

~~~text
Command or Query
       │
       ▼
FluentValidation validator ── invalid ──► Result.Invalid
       │ коректний вхід
       ▼
Handler ──► Domain aggregate / Application abstraction
       │
       └──► Result.Success | Result.NotFound | Result.Conflict | Result.Invalid
~~~

## Правила

- Команди змінюють стан, запити лише читають.
- Обробник виконує один сценарій використання й отримує лише абстракції.
- Запит застосовує проєкцію без відстеження змін, коли це доречно.
- Валідатори відповідають за вхід; Domain — за бізнес-інваріанти.
- Публічний міжмодульний DTO розміщуй у Module.Contracts, а не поряд із handler.
