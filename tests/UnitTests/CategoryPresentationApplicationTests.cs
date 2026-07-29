using System.Reflection;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category.GetList;
using Catalog.Application.Features.Category.GetList.DTOs;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace UnitTests;

public sealed class CategoryPresentationApplicationTests
{
    [Theory]
    [InlineData("000005513", 5513, "Фурнітура", "Фурнитура", 0)]
    [InlineData("000007226", 7226, "Двері", "Двери", 1)]
    [InlineData("Космос", 920000, "Космос", "Космос", 2)]
    public void Resolve_ReturnsConfiguredHardwareDoorsAndCosmosMetadata(
        string root,
        int categoryId,
        string shortNameUk,
        string shortNameRu,
        int sortOrder)
    {
        var resolver = new CategoryPresentationResolver(new CategoryPresentationOptions
        {
            Categories =
            [
                new() { ProductTypeIdOneC = "000005513", CategoryId = 5513, ShortNameUk = "Фурнітура", ShortNameRu = "Фурнитура", SortOrder = 0 },
                new() { ProductTypeIdOneC = "000007226", CategoryId = 7226, ShortNameUk = "Двері", ShortNameRu = "Двери", SortOrder = 1 },
                new() { ProductTypeIdOneC = "Космос", CategoryId = 920000, ShortNameUk = "Космос", ShortNameRu = "Космос", SortOrder = 2 }
            ]
        });

        var resolved = resolver.Resolve(root, categoryId, "1C source name", 0);

        Assert.Equal(shortNameUk, resolved.ShortNameUk);
        Assert.Equal(shortNameRu, resolved.ShortNameRu);
        Assert.Equal(sortOrder, resolved.SortOrder);
        Assert.Equal(0, resolved.Level);
    }

    [Fact]
    public void Resolve_UsesSourceNameAndLastSortOrderForUnknownCategory()
    {
        var resolver = new CategoryPresentationResolver(new CategoryPresentationOptions());

        var resolved = resolver.Resolve("000005513", 999999, "Нова категорія 1С", 2);

        Assert.Equal("Нова категорія 1С", resolved.ShortNameUk);
        Assert.Equal("Нова категорія 1С", resolved.ShortNameRu);
        Assert.Equal(int.MaxValue, resolved.SortOrder);
        Assert.Equal(2, resolved.Level);
    }

    [Fact]
    public async Task GetCategoriesQuery_BuildsSortOrderFirstTreeWithMetadata()
    {
        var root = CreateCategory(5513, "n5513", "Root", 0, 0);
        var second = CreateCategory(900002, "n5513.n900002", "Second", 2, 1);
        var first = CreateCategory(900001, "n5513.n900001", "First", 1, 1);
        var proxy = DispatchProxy.Create<ICatalogReadRepository<ProductCategory>, ReadRepositoryProxy>();
        ((ReadRepositoryProxy)(object)proxy).Categories = [root, second, first];
        var handler = new GetCategoriesQuryHandler(proxy);

        var result = await handler.Handle(new GetCategoriesQuery(new CategoryFilter(null)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tree = Assert.Single(result.Value);
        Assert.Equal("Root", tree.Name);
        Assert.Equal([900001, 900002], tree.Children.Select(child => child.Id));
        Assert.Equal("First uk", tree.Children[0].ShortNameUk);
        Assert.Equal(1, tree.Children[0].Level);
    }

    private static ProductCategory CreateCategory(int id, string path, string name, int sortOrder, int level)
    {
        var category = ProductCategory.Create(
            ProductCategoryId.From(id),
            "000005513",
            name,
            $"category-{id}",
            CategoryPath.From(path));
        category.UpdatePresentationMetadata($"{name} uk", $"{name} ru", sortOrder, level);
        return category;
    }

    private class ReadRepositoryProxy : DispatchProxy
    {
        public IReadOnlyList<ProductCategory> Categories { get; set; } = [];

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "ListAsync")
                return Task.FromResult(Categories.ToList());

            throw new NotSupportedException($"Unexpected repository call: {targetMethod?.Name}");
        }
    }
}
