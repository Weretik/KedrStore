# ADR 0022: Use Fluent Validation

## Date
2025-06-17

## Status
Accepted

## Context
Validation is a critical aspect of any application, ensuring that data conforms to expected formats and rules before processing. Fluent Validation is a popular library for .NET that provides a fluent API for defining validation rules in a clean and expressive manner. In the Kedr E-Commerce Platform, Fluent Validation simplifies validation logic and ensures consistency across the application.

## Decision
We decided to use Fluent Validation in the project to:

1. Simplify the definition of validation rules using a fluent API.
2. Centralize validation logic for better maintainability.
3. Ensure consistency in validation across the application.
4. Align with modern .NET practices and leverage community-supported libraries.

## Consequences
### Positive
1. Simplifies validation logic with a clean and expressive API.
2. Improves maintainability by centralizing validation rules.
3. Ensures consistency in validation across different layers.
4. Reduces boilerplate code compared to manual validation.

### Negative
1. Adds a dependency on Fluent Validation, which must be maintained and updated.
2. Requires developers to learn the Fluent Validation API.
3. May introduce runtime errors if validation rules are misconfigured.

## Example
Validation is implemented using Fluent Validation as follows:

**Validator Implementation**:
```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Product name must not be empty.");

        RuleFor(p => p.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than zero.");

        RuleFor(p => p.CategoryId)
            .NotNull()
            .WithMessage("Product must have a category.");
    }
}
```

**Usage in Application Layer**:
```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result>
{
    private readonly IValidator<Product> _validator;

    public CreateProductCommandHandler(IValidator<Product> validator)
    {
        _validator = validator;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request.Product, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        // Proceed with creating the product...
        return Result.Success();
    }
}
```
