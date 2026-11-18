using BuildingBlocks.Domain.Identifiers;
using Ardalis.Result;
using BuildingBlocks.Application.Results;
using global::Catalog.Application.Features.Attributes.Commands.UpdateCatalogAttribute;
using global::Catalog.Application.Features.Attributes.Shared.Readers;
using global::Catalog.Application.Features.Categories.Commands.SaveCategoryContent;
using global::Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;
using global::Catalog.Application.Features.SiteModels.Shared;
using global::Catalog.Application.Features.SiteModels.Shared.State;
using global::Catalog.Contracts.Attributes;
using global::Catalog.Contracts.Category;
using global::Catalog.Contracts.SiteModels;
using global::Catalog.Domain.Attributes;
using global::Catalog.Domain.Attributes.Enums;
using global::Catalog.Domain.Categories;
using global::Catalog.Domain.Categories.Content;
using global::Catalog.Domain.Groups;
using global::Catalog.Infrastructure.Integrations.Html;
using global::Catalog.Infrastructure.Persistence;
using IntegrationTests.Catalog.ProductMedia;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Persistence;

/// <summary>Database rules 4: a Catalog form that read an older version cannot overwrite a newer save.</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class CatalogFormVersionPostgresTests(CatalogPostgresFixture postgres)
{
    private const int CategoryId = 20;

    [SkippableFact]
    public async Task CategoryContent_FirstSave_ThenAnOlderVersion_AnswersConflict()
    {
        postgres.SkipIfUnavailable();
        await SeedCategoryAsync();

        var first = await SaveContentAsync("Перший заголовок", 0);
        var fromEmptyForm = await SaveContentAsync("З порожньої форми", 0);
        var second = await SaveContentAsync("Друга правка", first.Value.Version);
        var stale = await SaveContentAsync("Стара форма", first.Value.Version);

        Assert.True(first.IsSuccess);
        Assert.NotEqual(0u, first.Value.Version);
        Assert.Equal(ResultStatus.Conflict, fromEmptyForm.Status);
        Assert.True(second.IsSuccess);
        Assert.Equal(ResultStatus.Conflict, stale.Status);
        Assert.Equal(RowVersions.ChangedMessage, Assert.Single(stale.Errors));
        await using var db = postgres.CreateContext();
        Assert.Equal("Друга правка", (await db.CategoryContents.AsNoTracking().SingleAsync()).SeoTitle);
    }

    [SkippableFact]
    public async Task SiteModelText_WithAnOlderVersion_AnswersConflict()
    {
        postgres.SkipIfUnavailable();
        await SeedCategoryAsync();
        int modelId;
        await using (var seed = postgres.CreateContext())
        {
            var group = ProductGroup.CreateManual(ProductCategoryId.Create(CategoryId), "Модель", "model-version",
                DateTimeOffset.UtcNow).Value;
            seed.ProductGroups.Add(group);
            await seed.SaveChangesAsync();
            modelId = group.Id.Value;
        }

        var first = await SaveTextAsync(modelId, "Перший опис", 0);
        var firstVersion = first.Value.Texts.Single(text => text.Language == "uk").Version;
        var second = await SaveTextAsync(modelId, "Другий опис", firstVersion);
        var stale = await SaveTextAsync(modelId, "Стара форма", firstVersion);

        Assert.NotEqual(0u, firstVersion);
        Assert.True(second.IsSuccess);
        Assert.Equal(ResultStatus.Conflict, stale.Status);
    }

    [SkippableFact]
    public async Task Attribute_WithAnOlderVersion_AnswersConflict_AndWithoutAVersionSaves()
    {
        postgres.SkipIfUnavailable();
        int attributeId;
        await using (var seed = postgres.CreateContext())
        {
            await seed.Database.ExecuteSqlRawAsync("""DELETE FROM "CatalogAttributes" WHERE "Code" = 'version-test';""");
            var attribute = CatalogAttribute.Create("version-test", "Колір", AttributeKind.Enum, null, "version-test",
                isFilterable: false, isVariantAxis: false, sortOrder: 1, DateTimeOffset.UtcNow).Value;
            seed.Attributes.Add(attribute);
            await seed.SaveChangesAsync();
            attributeId = attribute.Id.Value;
        }

        var first = await SaveAttributeAsync(attributeId, "Колір 1", null);
        var second = await SaveAttributeAsync(attributeId, "Колір 2", first.Value.Version);
        var stale = await SaveAttributeAsync(attributeId, "Колір 3", first.Value.Version);
        var withoutVersion = await SaveAttributeAsync(attributeId, "Колір 4", null);

        Assert.NotEqual(first.Value.Version, second.Value.Version);
        Assert.Equal(ResultStatus.Conflict, stale.Status);
        Assert.Equal("Колір 4", withoutVersion.Value.Attribute.NameUk);
    }

    private async Task SeedCategoryAsync()
    {
        await using var seed = postgres.CreateContext();
        await seed.Database.ExecuteSqlRawAsync(
            """TRUNCATE "CatalogProductGroups", "CategoryFaqItems", "CategoryContents", "ProductCategories" CASCADE;""");
        seed.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(CategoryId), "000005520", "Завіси", "zavisy",
            CategoryPath.Create("n20")).Value);
        await seed.SaveChangesAsync();
    }

    private async Task<Result<AdminCategoryContentItem>> SaveContentAsync(string seoTitle, uint? version)
    {
        await using var db = postgres.CreateContext();
        return await new SaveCategoryContentHandler(new TestRepository<CategoryContent>(db), db,
                new HtmlSanitizerService())
            .Handle(new SaveCategoryContentCommand(CategoryId, "uk",
                new SaveCategoryContentRequest(seoTitle, null, null, null, null, version), "manager"),
                CancellationToken.None);
    }

    private async Task<Result<SiteModelDto>> SaveTextAsync(int modelId, string description, uint? version)
    {
        await using var db = postgres.CreateContext();
        return await new SetSiteModelTextHandler(db, new TestRepository<ProductGroupText>(db),
                new SiteModelReader(new SiteModelStateReader(db)), TimeProvider.System)
            .Handle(new SetSiteModelTextCommand(modelId, "uk",
                new SetSiteModelTextRequest { ShortDescription = description, Version = version }),
                CancellationToken.None);
    }

    private async Task<Result<AdminCatalogAttributeDto>> SaveAttributeAsync(int attributeId, string nameUk,
        uint? version)
    {
        await using var db = postgres.CreateContext();
        return await new UpdateCatalogAttributeHandler(db, new TestRepository<CatalogAttribute>(db),
                new CatalogAttributeReader(db, new FakeProductMediaLibrary()), TimeProvider.System)
            .Handle(new UpdateCatalogAttributeCommand(attributeId,
                new UpdateCatalogAttributeRequest(nameUk, null, false, 1, version)), CancellationToken.None);
    }
}
