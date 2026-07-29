# ADR 0013: Use of Value Objects to Strengthen the Domain Model

## Status
Accepted

## Date
2025-06-01

## Context

During the development of the domain layer in KedrStore, we encountered the need to model a set of concepts that:

1. Have no unique identity  
2. Are defined by their attributes  
3. Are immutable  
4. Frequently encapsulate business rules and constraints  

Examples of such concepts in our domain:

- Money (amount + currency)  
- Email addresses  
- Shipping addresses  
- Entity identifiers  

Using primitive types for these concepts leads to problems:

- Loss of business rules and validation  
- Duplication of validation logic across the codebase  
- Lack of type safety (e.g., mixing different ID types)  
- Inability to encapsulate business logic inside domain types  

## Decision

We chose to use the **Value Object pattern** in accordance with DDD (Domain-Driven Design) principles:

1. Create an abstract base class `ValueObject` with equality and hash code logic  
2. Implement concrete classes for each semantic value in the domain  
3. Enforce immutability across all value objects  
4. Embed business validation logic within the value objects  

Example base class implementation:

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}
```

Example of a concrete value object for entity ID:

```csharp
public abstract class EntityId : ValueObject
{
    public int Value { get; }

    protected EntityId(int value)
    {
        if (value <= 0)
            throw new DomainException("ID must be greater than zero.");

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

public class ProductId : EntityId
{
    public ProductId(int value) : base(value) { }

    public static implicit operator int(ProductId id) => id.Value;
}
```

## Consequences

### Positive

- Improved type safety and prevention of accidental ID mixing  
- Encapsulated validation and business logic  
- More expressive and intention-revealing domain model  
- Easier to test (isolated, deterministic behavior)  
- Protection from unwanted mutations due to immutability  

### Negative

- Increased number of classes in the codebase  
- Slight performance cost of object creation  
- Requires value converters for ORM and API mapping  

## Implementation

1. The base `ValueObject` class was added to the Domain project  
2. Core value objects were implemented:  
   - `Money` (amount + currency)  
   - `Email` (with format validation)  
   - `PhoneNumber` (with validation/formatting)  
   - `Address` (with components)  
   - `ProductId`, `OrderId`, `CustomerId`, etc.  

3. Implicit conversion operators were implemented for developer convenience  
4. EF Core value converters were added to support persistence  

Example of EF Core configuration:

```csharp
modelBuilder.Entity<Product>().Property(p => p.Id)
    .HasConversion(
        v => v.Value,
        v => new ProductId(v));
```
