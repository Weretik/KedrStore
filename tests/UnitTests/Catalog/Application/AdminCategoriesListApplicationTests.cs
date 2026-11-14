using System.Reflection;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Categories.Queries.GetAdminCategories;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace UnitTests.Catalog.Application;

public sealed class AdminCategoriesListApplicationTests
{
    [Fact]
    public async Task ActiveList_AddsProductCountsWithDescendantsAndFilledLanguages()
    {
        var root = CreateCategory(5513, "n5513", 0, 0);
        var child = CreateCategory(900001, "n5513.n900001", 0, 1);
        var empty = CreateCategory(900002, "n5513.n900002", 1, 1);
        var handler = new GetAdminCategoriesHandler(
            Repository([root, child, empty]),
            new FixedInsights(
                new Dictionary<int, int> { [5513] = 1, [900001] = 4 },
                new Dictionary<int, IReadOnlyList<string>> { [900001] = ["ru", "uk"] }));

        var result = await handler.Handle(new GetAdminCategoriesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([5513, 900001, 900002], result.Value.Select(row => row.Id));
        Assert.Equal([5, 4, 0], result.Value.Select(row => row.ProductCount));
        Assert.Equal(["ru", "uk"], result.Value[1].FilledContentLanguages);
        Assert.Empty(result.Value[0].FilledContentLanguages!);
    }

    [Fact]
    public async Task DeletedList_DoesNotReadInsights()
    {
        var deleted = CreateCategory(900003, "n5513.n900003", 0, 1);
        var handler = new GetAdminCategoriesHandler(Repository([deleted]), new ThrowingInsights());

        var result = await handler.Handle(new GetAdminCategoriesQuery(Deleted: true), CancellationToken.None);

        var row = Assert.Single(result.Value);
        Assert.Null(row.ProductCount);
        Assert.Null(row.FilledContentLanguages);
    }

    private static ICatalogReadRepository<ProductCategory> Repository(IReadOnlyList<ProductCategory> categories)
    {
        var proxy = DispatchProxy.Create<ICatalogReadRepository<ProductCategory>, ReadRepositoryProxy>();
        ((ReadRepositoryProxy)(object)proxy).Categories = categories;
        return proxy;
    }

    private static ProductCategory CreateCategory(int id, string path, int sortOrder, int level)
    {
        var category = ProductCategory.Create(
            ProductCategoryId.From(id), "000005513", $"Category {id}", $"category-{id}", CategoryPath.From(path));
        category.UpdatePresentationMetadata($"Категорія {id}", $"Категория {id}", sortOrder, level);
        return category;
    }

    private sealed class FixedInsights(
        IReadOnlyDictionary<int, int> counts,
        IReadOnlyDictionary<int, IReadOnlyList<string>> languages) : IAdminCategoryInsightsReader
    {
        public Task<IReadOnlyDictionary<int, int>> LoadDirectProductCountsAsync(CancellationToken cancellationToken)
            => Task.FromResult(counts);

        public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> LoadFilledContentLanguagesAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(languages);
    }

    private sealed class ThrowingInsights : IAdminCategoryInsightsReader
    {
        public Task<IReadOnlyDictionary<int, int>> LoadDirectProductCountsAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("Deleted rows need no counts.");

        public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> LoadFilledContentLanguagesAsync(
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Deleted rows need no texts.");
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
