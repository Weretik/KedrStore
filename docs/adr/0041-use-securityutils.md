# ADR 0041: Use SecurityUtils

## Date
2025-06-17

## Status
Accepted

## Context
`SecurityUtils` is a utility class designed to provide security-related functionality, such as generating secure passwords. It simplifies the implementation of security features by centralizing common operations and ensuring consistency across the application. In the Kedr E-Commerce Platform, `SecurityUtils` is used to generate secure passwords for user accounts and other sensitive operations.

## Decision
We decided to use `SecurityUtils` in the project to:

1. Centralize security-related functionality for better maintainability.
2. Ensure consistency in the generation of secure passwords.
3. Simplify the implementation of security features across the application.
4. Align with best practices for secure application development.

## Consequences
### Positive
1. Improves maintainability by centralizing security-related logic.
2. Ensures consistency in the generation of secure passwords.
3. Simplifies debugging and testing of security features.
4. Aligns with best practices for secure application development.

### Negative
1. Adds complexity by introducing a custom utility class.
2. Requires careful implementation to ensure security and reliability.

## Example
`SecurityUtils` is implemented as follows:

**SecurityUtils.cs**:
```csharp
public static class SecurityUtils
{
    public static string GenerateSecurePassword(int length = 16)
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*()";
        var all = uppercase + lowercase + digits + symbols;

        var random = new Random();
        var password = new char[length];

        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = symbols[random.Next(symbols.Length)];

        for (int i = 4; i < length; i++)
            password[i] = all[random.Next(all.Length)];

        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }
}
```

**Usage in Application Layer**:
```csharp
public class UserService
{
    public string GenerateAdminPassword()
    {
        return SecurityUtils.GenerateSecurePassword();
    }
}
```
