# ADR 0053: Use .env Files for Environment Configuration

## Date
2025-07-28

## Status
Accepted

## Context
Managing configuration settings across different environments (development, testing, staging, production) is a crucial aspect of application development. Environment variables are commonly used for this purpose, but setting them up manually can be cumbersome and error-prone. The Kedr E-Commerce Platform requires a simple, portable solution for managing environment-specific configuration that works alongside our other configuration mechanisms.

## Decision
We decided to implement .env file support through a custom `Env.Load()` utility to:

1. Provide a convenient way to define environment-specific configuration variables in a `.env` file.
2. Simplify local development setup by allowing developers to maintain their own environment configuration.
3. Create a consistent approach to environment configuration across different deployment environments.
4. Complement our existing configuration approach using User Secrets (see ADR 0024) and appsettings.json.

## Consequences
### Positive
1. Simplifies environment configuration management, especially for local development.
2. Provides a standard way to handle environment-specific settings.
3. Makes it easy to onboard new developers with a template .env file.
4. Works well with Docker containerization, allowing injection of environment variables.
5. Reduces the risk of hardcoded sensitive information in the codebase.

### Negative
1. Introduces another configuration mechanism alongside User Secrets and appsettings.json.
2. Requires careful management to avoid committing .env files to version control.
3. May lead to inconsistency if not properly documented and maintained.

## Implementation

**Configuration in Program.cs**:
```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Загружаем .env локально
Env.Load();

// Конфигурация: переменные окружения → appsettings.json
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
```

The configuration precedence is established as follows:
1. Environment variables (including those loaded from .env)
2. User Secrets (in development)
3. appsettings.json

**Example .env File Structure**:
```
# Database Configuration
ConnectionStrings__DefaultConnection=Server=localhost;Database=KedrStore;User Id=kedruser;Password=password;

# External API Keys
PaymentGateway__ApiKey=your_api_key_here
PaymentGateway__SecretKey=your_secret_key_here

# Application Settings
App__SiteUrl=https://localhost:5001
App__AdminEmail=admin@kedr.ua
```
