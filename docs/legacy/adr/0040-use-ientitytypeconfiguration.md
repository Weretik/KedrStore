# ADR 0040: Use IEntityTypeConfiguration

## Date
2025-06-17

## Status
Accepted

## Context
`IEntityTypeConfiguration` is an interface provided by EF Core to define the configuration of entities in a clean and modular way. It allows developers to separate entity configuration logic from the domain model, ensuring better maintainability and alignment with the principles of Clean Architecture. In the Kedr E-Commerce Platform, `IEntityTypeConfiguration` is used to configure database mappings, relationships, and constraints for domain entities.

## Decision
We decided to use `IEntityTypeConfiguration` in the project to:

1. Separate entity configuration logic from the domain model.
2. Centralize database configuration for better maintainability.
3. Simplify the management of relationships, constraints, and mappings.
4. Align with best practices for clean and modular architecture.

## Consequences
### Positive
1. Improves maintainability by centralizing entity configuration logic.
2. Simplifies debugging and testing of database mappings.
3. Promotes separation of concerns by decoupling configuration from the domain model.
4. Aligns with EF Core best practices for entity configuration.

### Negative
1. Adds complexity by introducing additional configuration classes.
2. Requires developers to understand and correctly implement `IEntityTypeConfiguration`.

## Example
`IEntityTypeConfiguration` is implemented as follows:

**CategoryConfiguration.cs**:
```csharp
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new CategoryId(value));

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ParentCategoryId)
            .HasConversion(
                id => id != null ? id.Value : (int?)null,
                value => value != null ? new CategoryId(value.Value) : null);

        builder.HasMany(c => c.Children)
            .WithOne()
            .HasForeignKey(c => c.ParentCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentCategoryId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
```

**Registration in DbContext**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(CategoryConfiguration).Assembly);
}
```
