да# ADR 0024: Use User Secrets

## Date
2025-06-17

## Status
Accepted

## Context
Managing sensitive information, such as connection strings, API keys, and other secrets, is a critical aspect of application development. Storing these secrets securely and separately from the codebase ensures better security and maintainability. In the Kedr E-Commerce Platform, we use the User Secrets feature provided by .NET to manage sensitive information during development.

## Decision
We decided to use User Secrets in the project to:

1. Securely store sensitive information during development.
2. Ensure that secrets are not included in the codebase or version control.
3. Simplify the management of secrets by leveraging .NET's built-in User Secrets functionality.
4. Align with best practices for secure application development.

## Consequences
### Positive
1. Improves security by keeping sensitive information out of the codebase.
2. Simplifies secret management during development.
3. Aligns with .NET best practices for handling sensitive information.
4. Reduces the risk of accidental exposure of secrets in version control.

### Negative
1. Requires developers to configure User Secrets on their local machines.
2. Secrets must be managed separately for production environments.
3. Adds complexity to the development setup process.

## Example
User Secrets are implemented as follows:

**Configuration in appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "UserSecrets:DefaultConnection"
  }
}
```

**Accessing Secrets in Code**:
```csharp
public class DatabaseSettings
{
    public string DefaultConnection { get; set; } = string.Empty;
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

var databaseSettings = builder.Configuration.GetSection("ConnectionStrings").Get<DatabaseSettings>();
Console.WriteLine(databaseSettings.DefaultConnection);
```

**Setting Secrets Locally**:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Kedr;User Id=admin;Password=secret;"
```
