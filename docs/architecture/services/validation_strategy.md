# Validation Strategy – KedrStore

## Overview

This document describes the validation architecture in KedrStore, based entirely on the actual implementation in the project. Validation is structured by layer, uses FluentValidation for input checks, and integrates tightly with the MediatR pipeline using behaviors. Validation never throws exceptions — instead, it returns `AppError` wrapped in `AppResult<T>`.

---

## Validation Layers

```text
┌────────────────────────────────────┐
│  Presentation Layer (Blazor UI)   │
│  - Receives AppResult             │
│  - Can display AppError.Message   │
│  - Supports ValidationErrors dict │
└──────────────┬────────────────────┘
               │
               ▼
┌────────────────────────────────────┐
│  Application Layer                 │
│  - Validators (FluentValidation)   │
│  - ValidationBehavior<T>          │
│  - AppResult.Failure(AppError)    │
└──────────────┬────────────────────┘
               │
               ▼
┌────────────────────────────────────┐
│  Domain Layer                      │
│  - RuleChecker + IBusinessRule     │
│  - For deep domain invariants      │
└────────────────────────────────────┘
```

---

## FluentValidation Setup

Validators are located in:
```text
Application/UseCases/<Feature>/Validators/
```

### Example: `CreateProductCommandValidator.cs`

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
```

- Validators are discovered automatically by `ValidationBehavior`
- All validation failures are wrapped into `AppError.Validation(...)`
- Errors are returned as a dictionary: `Dictionary<string, string[]>`

---

## ValidationBehavior

Location: `Application/Common/Behaviors/ValidationBehavior.cs`

Intercepts MediatR pipeline and runs all registered validators:

```csharp
var failures = ... // from FluentValidation

var errors = failures
    .GroupBy(f => f.PropertyName)
    .ToDictionary(
        g => g.Key,
        g => g.Select(x => x.ErrorMessage).ToArray()
    );

return AppResult.Failure<TResponse>(AppError.Validation(errors));
```

---

## AppError with ValidationErrors

```csharp
public sealed record AppError(string Code, string Message)
{
    public IDictionary<string, string[]>? ValidationErrors { get; private init; }

    public static AppError Validation(IDictionary<string, string[]> errors) =>
        new("Validation", "Validation error") with { ValidationErrors = errors };
}
```

---

## Example Use Case Validation

```csharp
public async Task<AppResult<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
{
    if (await _repository.ExistsByNameAsync(request.Name))
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = new[] { "Product with this name already exists." }
        };
        return AppResult.Failure<ProductDto>(AppError.Validation(errors));
    }

    // continue...
}
```

---

## Example Razor UI Rendering

```razor
@if (error?.ValidationErrors is not null)
{
    foreach (var field in error.ValidationErrors)
    {
        <p><strong>@field.Key:</strong></p>
        <ul>
            @foreach (var msg in field.Value)
            {
                <li>@msg</li>
            }
        </ul>
    }
}
```

Optional: extract this into `AppValidationSummary.razor` for reuse.

---

## Validation Testing

- Validators are tested via xUnit directly
- Use `TestValidationResult` pattern or just call `.Validate(command)`

```csharp
var validator = new CreateProductCommandValidator();
var result = validator.Validate(new CreateProductCommand(...));
result.Errors.Should().BeEmpty();
```

---

## Recommendations

- Use FluentValidation for all Commands and Queries
- Return `AppError.Validation(...)` with `Dictionary<string, string[]>`
- Map validation errors in UI into proper field-bound messages
- Enforce business invariants in Domain via `IBusinessRule`

---

## Summary

Validation in KedrStore is layered, testable, and safe. It ensures input data correctness via FluentValidation and guards domain rules separately. All failures are converted to `AppError`, with rich validation details, making the system predictable, maintainable, and UI-friendly.

