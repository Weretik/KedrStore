using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using global::Catalog.Application.Features.Attributes.Shared;
using global::Catalog.Application.Features.Attributes.Commands.CreateCatalogAttribute;
using global::Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;
using global::Catalog.Application.Features.Attributes.Commands.SetCategoryAttributeFilters;
using global::Catalog.Application.Features.ProductAttributes.Commands.AcceptProductReview;
using global::Catalog.Application.Features.ProductAttributes.Shared;
using global::Catalog.Application.Features.ProductAttributes.Commands.ReopenProductReview;
using global::Catalog.Application.Features.ProductAttributes.Commands.RestoreProductAttribute;
using global::Catalog.Application.Features.ProductAttributes.Commands.SetProductAttribute;
using global::Catalog.Contracts.Attributes;
using global::Catalog.Domain;
using global::Catalog.Domain.Attributes;
using global::Catalog.Domain.Categories;
using global::Catalog.Domain.Categories.Content;
using global::Catalog.Domain.Groups;
using global::Catalog.Domain.PriceTypes;
using global::Catalog.Domain.Products;
using global::Catalog.Domain.Products.Media;
using global::Catalog.Domain.Products.Prices;
using global::Catalog.Domain.Slugs;
using global::Catalog.Domain;
using global::Catalog.Domain.Attributes;
using global::Catalog.Domain.Categories;
using global::Catalog.Domain.Categories.Content;
using global::Catalog.Domain.Groups;
using global::Catalog.Domain.PriceTypes;
using global::Catalog.Domain.Products;
using global::Catalog.Domain.Products.Media;
using global::Catalog.Domain.Products.Prices;
using global::Catalog.Domain.Slugs;
using global::Catalog.Domain;
using global::Catalog.Domain.Attributes;
using global::Catalog.Domain.Categories;
using global::Catalog.Domain.Categories.Content;
using global::Catalog.Domain.Groups;
using global::Catalog.Domain.PriceTypes;
using global::Catalog.Domain.Products;
using global::Catalog.Domain.Products.Media;
using global::Catalog.Domain.Products.Prices;
using global::Catalog.Domain.Slugs;
using global::Catalog.Infrastructure.Adapters.Media;
using global::Catalog.Infrastructure.Contracts.Products;
using global::Catalog.Infrastructure.Contracts.Reference;
using global::Catalog.Infrastructure.Contracts.Sales;
using global::Catalog.Infrastructure.DependencyInjection;
using global::Catalog.Infrastructure.Integrations.Excel;
using global::Catalog.Infrastructure.Integrations.Html;
using global::Catalog.Infrastructure.Integrations.OneC;
using global::Catalog.Infrastructure.Integrations.Telegram;
using global::Catalog.Infrastructure.Persistence;
using global::Catalog.Infrastructure.Persistence.Attributes;
using global::Catalog.Infrastructure.Persistence.Categories;
using global::Catalog.Infrastructure.Persistence.Categories.Content;
using global::Catalog.Infrastructure.Persistence.Groups;
using global::Catalog.Infrastructure.Persistence.PriceTypes;
using global::Catalog.Infrastructure.Persistence.Products;
using global::Catalog.Infrastructure.Persistence.Products.Prices;
using global::Catalog.Infrastructure.Persistence.Slugs;
using global::Infrastructure;
using IntegrationTests.TestSupport.Content;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Attributes;

/// <summary>Spec catalog/006 phase 3: manual values, review decisions, code decoding, and category filters.</summary>
public sealed class AttributeAdminTests
{
    private static string Csv(params string[] rows) => CsvHeader + "\n" + string.Join("\n", rows) + "\n";

    private static readonly string Rows = Csv(
        Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "CR", "L"), Row(3, "Завіса", "AB", "R", reasonCode: "unresolved_color"));

    private static async Task<CatalogDbContext> ImportedAsync()
    {
        var db = CreateContext();
        AddCategory(db, 10);
        AddProduct(db, 1, 10, "Ручка AB L");
        AddProduct(db, 2, 10, "Ручка CP L");
        AddProduct(db, 3, 10, "Завіса AB R");
        await db.SaveChangesAsync();
        await RunAsync(db, Dictionary, Rows, apply: true);
        return db;
    }

    private static async Task<int> ValueIdAsync(CatalogDbContext db, string code)
        => (await db.AttributeValues.AsNoTracking().SingleAsync(value => value.Code == code)).Id.Value;

    private static async Task<int> AttributeIdAsync(CatalogDbContext db, string code)
        => (await db.Attributes.AsNoTracking().SingleAsync(attribute => attribute.Code == code)).Id.Value;

    private static async Task<List<ProductAttributeValue>> ColorsAsync(CatalogDbContext db, int productId)
    {
        var color = AttributeId.Create(await AttributeIdAsync(db, "color"));
        return await db.ProductAttributeValues.AsNoTracking()
            .Where(value => value.ProductId == ProductId.From(productId) && value.AttributeId == color)
            .OrderBy(value => value.Position).ToListAsync();
    }

    private static async Task<IReadOnlyList<AttributeReviewReason>> ReasonsAsync(CatalogDbContext db, int productId)
        => (await db.ProductAttributeReviews.AsNoTracking().SingleAsync(review => review.ProductId == ProductId.From(productId)))
            .Reasons;

    [Fact]
    public async Task YesNoAttribute_IsCreatedWithTheValuesTakAndNi()
    {
        await using var db = await ImportedAsync();
        var create = new CreateCatalogAttributeCommandHandler(db, new TestRepository<CatalogAttribute>(db),
            new TestRepository<AttributeValue>(db),
            new CatalogAttributeReader(db, new global::IntegrationTests.Catalog.ProductMedia.FakeProductMediaLibrary()),
            TimeProvider.System);

        var result = await create.Handle(new CreateCatalogAttributeCommand(new CreateCatalogAttributeRequest(
            "Пакування", null, CreateCatalogAttributeCommand.YesNo, null, null, null, true, false, 100)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("enum", result.Value.Attribute.Kind);
        Assert.Equal(["Так", "Ні"], result.Value.Values.Select(value => value.NameUk));
    }

    [Fact]
    public async Task ManualValue_IsLocked_KeptByImport_AndRestoredByRevert()
    {
        await using var db = await ImportedAsync();
        var color = await AttributeIdAsync(db, "color");
        var set = new SetProductAttributeCommandHandler(db, new TestRepository<ProductAttributeValue>(db),
            GroupBuilder(db), new ProductAttributesReader(db), TimeProvider.System);

        // Product 1 becomes chrome like product 2: same color and side → «Однакові варіанти».
        var result = await set.Handle(new SetProductAttributeCommand(1, color,
            new SetProductAttributeRequest([new SetProductAttributeValueRequest(await ValueIdAsync(db, "CR"), null)])),
            CancellationToken.None);
        Assert.True(result.IsSuccess);
        var dto = result.Value.Attributes.Single(attribute => attribute.Code == "color").Values.Single();
        Assert.Equal(("Хром", "manual", true, true), (dto.ValueName, dto.Source, dto.IsLocked, dto.HasAutomatic));
        Assert.Equal("Ручка AB L", result.Value.OriginalName);
        db.ChangeTracker.Clear();
        Assert.Contains(await ReasonsAsync(db, 1), reason => reason.Code == AttributeReviewReason.SameVariants);

        await RunAsync(db, Dictionary, Rows, apply: true);
        var kept = Assert.Single(await ColorsAsync(db, 1));
        Assert.Equal((AttributeValueSource.Manual, await ValueIdAsync(db, "CR")), (kept.Source, kept.ValueId!.Value.Value));

        var restore = new RestoreProductAttributeCommandHandler(db, new TestRepository<ProductAttributeValue>(db),
            GroupBuilder(db), new ProductAttributesReader(db), TimeProvider.System);
        var restored = await restore.Handle(new RestoreProductAttributeCommand(1, color), CancellationToken.None);
        Assert.True(restored.IsSuccess);
        db.ChangeTracker.Clear();
        var back = Assert.Single(await ColorsAsync(db, 1));
        Assert.Equal((AttributeValueSource.Import, false, await ValueIdAsync(db, "AB")),
            (back.Source, back.IsLocked, back.ValueId!.Value.Value));
        Assert.DoesNotContain(await ReasonsAsync(db, 1), reason => reason.Code == AttributeReviewReason.SameVariants);
    }

    [Fact]
    public async Task SetValue_RejectsAValueOfAnotherAttribute_AndAnEmptyList()
    {
        await using var db = await ImportedAsync();
        var set = new SetProductAttributeCommandHandler(db, new TestRepository<ProductAttributeValue>(db),
            GroupBuilder(db), new ProductAttributesReader(db), TimeProvider.System);

        var wrong = await set.Handle(new SetProductAttributeCommand(1, await AttributeIdAsync(db, "color"),
            new SetProductAttributeRequest([new SetProductAttributeValueRequest(await ValueIdAsync(db, "L"), null)])),
            CancellationToken.None);
        Assert.False(wrong.IsSuccess);

        var validator = new SetProductAttributeCommandValidator();
        Assert.False(validator.Validate(new SetProductAttributeCommand(1, 1, new SetProductAttributeRequest([]))).IsValid);
        Assert.False(validator.Validate(new SetProductAttributeCommand(1, 1,
            new SetProductAttributeRequest([new SetProductAttributeValueRequest(1, 2m)]))).IsValid);
    }

    [Fact]
    public async Task AcceptedProduct_StaysAcceptedAfterImport_UntilReopened()
    {
        await using var db = await ImportedAsync();
        var accept = new AcceptProductReviewCommandHandler(db,
            new TestRepository<ProductAttributeReview>(db), new ProductAttributesReader(db), new TestContentEditor(),
            TimeProvider.System);

        var accepted = await accept.Handle(new AcceptProductReviewCommand(3), CancellationToken.None);
        Assert.Equal(("accepted", TestContentEditor.EmailAddress),
            (accepted.Value.Review.State, accepted.Value.Review.AcceptedBy));

        await RunAsync(db, Dictionary, Rows, apply: true);
        var review = await db.ProductAttributeReviews.AsNoTracking().SingleAsync(item => item.ProductId == ProductId.From(3));
        Assert.Equal(AttributeReviewState.Accepted, review.State);

        var reopen = new ReopenProductReviewCommandHandler(db, new TestRepository<ProductAttributeReview>(db),
            new ProductAttributesReader(db), TimeProvider.System);
        var reopened = await reopen.Handle(new ReopenProductReviewCommand(3), CancellationToken.None);
        Assert.Equal("needs_review", reopened.Value.Review.State);
        Assert.Null(reopened.Value.Review.AcceptedBy);
    }

    [Fact]
    public async Task DecidingAnAssistantCode_ConfirmsItsProductValues_AndLeavesTheList()
    {
        await using var db = await ImportedAsync();
        var codes = new UnresolvedCodeReader(db, new CodeMeaningReader(db));
        var before = await codes.ListAsync(CancellationToken.None);
        var cp = Assert.Single(before, code => code.RawValue == "CP");
        Assert.Contains(before, code => code.RawValue == "AL" && code.ValueId is null);
        Assert.False(Assert.Single(await ColorsAsync(db, 2)).IsConfirmed);

        var decide = new DecideAttributeCodeCommandHandler(db, new TestRepository<AttributeAlias>(db),
            new TestRepository<AttributeAliasCategoryValue>(db), new TestRepository<AttributeAliasMeaning>(db), codes,
            RecognitionJob(db), TimeProvider.System, NullLogger<DecideAttributeCodeCommandHandler>.Instance);
        var result = await decide.Handle(new DecideAttributeCodeCommand(cp.AliasId,
            new DecideAttributeCodeRequest(await ValueIdAsync(db, "CR"))), CancellationToken.None);

        Assert.Equal(1, result.Value.ValuesConfirmed);
        Assert.True(result.Value.Code.IsConfirmed);
        db.ChangeTracker.Clear();
        Assert.True(Assert.Single(await ColorsAsync(db, 2)).IsConfirmed);
        Assert.DoesNotContain(await codes.ListAsync(CancellationToken.None), code => code.RawValue == "CP");

        // The dictionary import no longer overrides the owner's decision.
        await RunAsync(db, Dictionary, Rows, apply: true);
        var alias = await db.AttributeAliases.AsNoTracking().SingleAsync(item => item.RawValue == "CP");
        Assert.True(alias.IsConfirmed && alias.IsEditedManually);

        var wrongValue = await decide.Handle(new DecideAttributeCodeCommand(cp.AliasId,
            new DecideAttributeCodeRequest(await ValueIdAsync(db, "L"))), CancellationToken.None);
        Assert.False(wrongValue.IsSuccess);
    }

    [Fact]
    public async Task UnresolvedCodes_CountOnlyOnSiteProducts()
    {
        await using var db = await ImportedAsync();
        // A review reason that names the code, as the recognizer writes it.
        var mention = await db.ProductAttributeReviews.SingleAsync(item => item.ProductId == ProductId.From(2));
        mention.SetSameVariants("Код «CP»", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var codes = new UnresolvedCodeReader(db, new CodeMeaningReader(db));
        Assert.Equal(1, Assert.Single(await codes.ListAsync(CancellationToken.None), code => code.RawValue == "CP").ProductCount);

        var product = await db.Products.SingleAsync(item => item.Id == ProductId.From(2));
        product.UpdateFromOneC(product.Name, product.CategoryId, 1, exportToSite: false, false, false, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        Assert.Equal(0, Assert.Single(await codes.ListAsync(CancellationToken.None), code => code.RawValue == "CP").ProductCount);
    }

    [Fact]
    public async Task CategoryFilters_SetByAdmin_SurviveTheImport()
    {
        await using var db = await ImportedAsync();
        var reader = new CategoryFilterReader(db);
        var filters = await reader.ReadAsync(ProductCategoryId.From(10), CancellationToken.None);
        var color = Assert.Single(filters!, filter => filter.Code == "color");
        Assert.True(color.IsFilterable);
        Assert.Equal(3, color.ProductCount);

        var set = new SetCategoryAttributeFiltersCommandHandler(db, new TestRepository<CategoryAttribute>(db), reader,
            TimeProvider.System);
        var result = await set.Handle(new SetCategoryAttributeFiltersCommand(10, new SetCategoryAttributeFiltersRequest(
            [new SetCategoryAttributeFilterRequest(color.AttributeId, false, 5)])), CancellationToken.None);
        Assert.True(result.IsSuccess);

        await RunAsync(db, Dictionary, Rows, apply: true);
        var after = Assert.Single((await reader.ReadAsync(ProductCategoryId.From(10), CancellationToken.None))!,
            filter => filter.Code == "color");
        Assert.Equal((false, 5, true), (after.IsFilterable, after.SortOrder, after.IsEditedManually));
    }
}
