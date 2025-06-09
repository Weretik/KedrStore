# Використання User Secrets в проекті

## Що таке User Secrets?

User Secrets (Секрети користувача) - це механізм ASP.NET Core для зберігання конфіденційних даних під час розробки. Секрети зберігаються поза проектом, щоб вони не потрапили до системи контролю версій.

## Налаштування User Secrets

### Крок 1: Ініціалізація User Secrets

Виконайте наступну команду в директорії проекту:

```bash
dotnet user-secrets init
```

Це створить унікальний ідентифікатор для вашого проекту і додасть його до файлу .csproj.

### Крок 2: Додавання секретів

Додайте секрети за допомогою командного рядка:

```bash
dotnet user-secrets set "Identity:AdminUser:DefaultPassword" "YourStrongPassword!"
```

Або додайте кілька значень з JSON-файлу:

```bash
type secrets.json | dotnet user-secrets set
```

## Використання секретів в проекті

Секрети автоматично додаються до конфігурації через `AddUserSecrets<Program>()` в `Program.cs`.

Приклад використання в коді:

```csharp
var adminPassword = configuration["Identity:AdminUser:DefaultPassword"];
```

## Де зберігаються секрети?

User Secrets зберігаються на вашому комп'ютері в наступних локаціях:

- Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

## Безпека в Production-середовищі

User Secrets призначені тільки для розробки. В Production використовуйте:

- Змінні середовища
- Azure Key Vault
- Інші провайдери секретів

## Додаткова інформація

Документація Microsoft: [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
