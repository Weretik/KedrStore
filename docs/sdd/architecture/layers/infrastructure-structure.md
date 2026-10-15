# Структура Infrastructure

## Відповідальність

Infrastructure — шар адаптерів. Він реалізує контракти Application і зберігає дані/взаємодіє з конкретними технологіями.

## Цільова структура модуля

~~~text
<Module>.Infrastructure/
├── DataBase/
│   └── <Module>DbContext.cs
├── Configurations/          класи EF IEntityTypeConfiguration
├── Migrations/              EF-міграції та model snapshot
├── Repositories/            EF-реалізації репозиторіїв
├── Projections/             перебудова та зберігання моделей читання
├── Integrations/
│   └── <System>/            реалізації HTTP/SOAP/queue адаптерів
├── Services/                адаптери storage, export, notification
├── DependencyInjection/     extension-методи Add<Module>...
└── <Module>.Infrastructure.csproj
~~~

## Схема адаптерів

~~~text
Абстракція Application
          ▲
          │ реалізує
Адаптер Infrastructure
     ├── EF Core DbContext ──► PostgreSQL
     ├── зовнішній клієнт ────► 1C / Telegram / storage
     └── побудова проєкції ──► модель читання
~~~

## Правила

- EF-конфігурації та міграції не переходять до Domain або Application.
- Реєструй адаптери через чинний DI extension модуля.
- Інфраструктурний шар може посилатися на прикладний і доменний; зворотний напрям заборонений.
- Перетворення зовнішніх даних залишай на межі адаптера; не пропускай згенеровані клієнтські типи у внутрішні шари.
