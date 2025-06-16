# ADR 0016: Use Global Usings

## Date
2025-06-17

## Status
Accepted

## Context
In modern C# projects, global usings provide a way to simplify and centralize commonly used namespaces across the application. This feature reduces boilerplate code and improves readability by eliminating repetitive `using` directives in individual files.

## Decision
We decided to use global usings in the project to:

1. Centralize commonly used namespaces in a single file (`GlobalUsings.cs`) for each layer (Application, Domain, Infrastructure).
2. Reduce repetitive `using` directives in individual files.
3. Improve code readability and maintainability.
4. Align with modern C# practices introduced in .NET 6 and later.

## Consequences
### Positive
1. Simplifies code by reducing repetitive `using` directives.
2. Improves readability and maintainability.
3. Centralizes namespace management, making it easier for developers to understand dependencies.
4. Aligns with modern C# practices, ensuring compatibility with future updates.

### Negative
1. Requires careful management of global usings to avoid namespace conflicts.
2. Developers must explicitly add uncommon namespaces to individual files, which may lead to confusion.
3. May introduce subtle bugs if global usings unintentionally override local ones.

## Example
Each layer (Application, Domain, Infrastructure) will have its own `GlobalUsings.cs` file. For example:

**Application/GlobalUsings.cs**:
```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
```

**Domain/GlobalUsings.cs**:
```csharp
global using System;
global using System.Collections.Generic;
```

**Infrastructure/GlobalUsings.cs**:
```csharp
global using System;
global using System.Collections.Generic;
global using Microsoft.EntityFrameworkCore;
```

This structure ensures that each layer has access to the namespaces it commonly uses without redundancy.
