using Catalog.Domain.Entities;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IntegrationTests;

public sealed class CategoryPresentationPersistenceTests
{
    [Fact]
    public void CatalogModel_MapsRequiredPresentationMetadataAndTreeOrderingIndex()
    {
        using var dbContext = CreateDbContext();
        var category = dbContext.Model.FindEntityType(typeof(ProductCategory));

        Assert.NotNull(category);
        AssertRequiredString(category, nameof(ProductCategory.ShortNameUk));
        AssertRequiredString(category, nameof(ProductCategory.ShortNameRu));
        AssertRequiredInteger(category, nameof(ProductCategory.SortOrder));
        AssertRequiredInteger(category, nameof(ProductCategory.Level));

        Assert.Contains(category.GetIndexes(), index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ProductCategory.ParentId), nameof(ProductCategory.SortOrder), nameof(ProductCategory.Id)]));
    }

    [Fact]
    public void CatalogMigrations_ContainCategoryPresentationMetadataMigration()
    {
        using var dbContext = CreateDbContext();

        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_AddCategoryPresentationMetadata", StringComparison.Ordinal));
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=kedr_test;Username=kedr_user;Password=not-used")
            .Options;

        return new CatalogDbContext(options);
    }

    private static void AssertRequiredString(IEntityType category, string propertyName)
    {
        var property = category.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(ProductCategory.ShortNameMaxLength, property.GetMaxLength());
    }

    private static void AssertRequiredInteger(IEntityType category, string propertyName)
    {
        var property = category.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(typeof(int), property.ClrType);
    }
}
