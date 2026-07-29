# Business Rules – KedrStore

## Overview

This document describes the domain validation strategy in the KedrStore project, based on the implementation of `IBusinessRule` and `RuleChecker`. Business rules are used in the **Domain Layer** to ensure consistency and integrity of aggregates. They are strictly separated from input validation (which is handled in the Application Layer by FluentValidation).

---

## Motivation

- Keep domain logic pure and encapsulated
- Express complex invariants declaratively
- Reuse validation logic across aggregates
- Make rule violations explicit and testable

---

## Architecture Layer Placement

```text
┌────────────────────────────────────┐
│  Application Layer                 │
│  - FluentValidation (input)        │
│  - AppResult / AppError            │
└──────────────┬────────────────────┘
               │
               ▼
┌────────────────────────────────────┐
│  Domain Layer                      │
│  - IBusinessRule                   │
│  - RuleChecker                     │
│  - BusinessRuleValidationException │
└────────────────────────────────────┘
```

---

## Components

### 1. `IBusinessRule`

Location: `Domain/Common/Rules/IBusinessRule.cs`

```csharp
public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
```

Defines a reusable and testable contract for domain constraints.

---

### 2. `RuleChecker`

Location: `Domain/Common/Rules/RuleChecker.cs`

```csharp
public static class RuleChecker
{
    public static void Check(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}
```

Use this statically in your aggregate methods or constructors:

```csharp
RuleChecker.Check(new ProductNameMustBeUniqueRule(name, _repository));
```

---

### 3. `BusinessRuleValidationException`

Location: `Domain/Common/Rules/BusinessRuleValidationException.cs`

```csharp
public sealed class BusinessRuleValidationException : Exception
{
    public string Details { get; }

    public BusinessRuleValidationException(IBusinessRule rule)
        : base(rule.Message)
    {
        Details = rule.Message;
    }
}
```

This is the only exception that can be thrown in the Domain layer.

---

## Example Rule

### `CategoryMustExistRule`

Location: `Domain/Catalog/Rules/CategoryMustExistRule.cs`

```csharp
public class CategoryMustExistRule : IBusinessRule
{
    private readonly ICategoryRepository _repository;
    private readonly CategoryId _categoryId;

    public CategoryMustExistRule(ICategoryRepository repository, CategoryId categoryId)
    {
        _repository = repository;
        _categoryId = categoryId;
    }

    public bool IsBroken() => !_repository.Exists(_categoryId);

    public string Message => $"Category with ID {_categoryId.Value} does not exist.";
}
```

Used in `Product` aggregate constructor or factory.

---

## Usage Pattern

In aggregate methods or constructors:

```csharp
public Product(...)
{
    RuleChecker.Check(new CategoryMustExistRule(repository, categoryId));
    // Assign fields...
}
```

Domain rules are enforced **before any state mutation**.

---

## Testing Rules

Each rule can and should be tested in isolation:

```csharp
[Fact]
public void CategoryMustExistRule_Should_Fail_When_Not_Found()
{
    var fakeRepo = Substitute.For<ICategoryRepository>();
    fakeRepo.Exists(Arg.Any<CategoryId>()).Returns(false);

    var rule = new CategoryMustExistRule(fakeRepo, new CategoryId(Guid.NewGuid()));
    rule.IsBroken().Should().BeTrue();
    rule.Message.Should().Contain("does not exist");
}
```

---

## Summary

KedrStore uses `IBusinessRule` and `RuleChecker` to enforce domain invariants declaratively and testably. These rules express what **must be true** for entities and aggregates to be in a valid state, without mixing concerns from infrastructure or application layers.

