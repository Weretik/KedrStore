using Xunit.Abstractions;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Attributes;

/// <summary>Catalog attributes 006, phase 1: dictionary and CSV import (SC-A01–SC-A06).</summary>
public sealed class AttributeImportTests(ITestOutputHelper output)
{
    private static string Csv(params string[] rows) => string.Join("\n", rows.Prepend(CsvHeader)) + "\n";

    private static async Task<CatalogDbContext> SeededAsync()
    {
        var db = CreateContext();
        AddCategory(db, 10);
        AddProduct(db, 1, 10, "KEDR ручка 42-323-AB-L");
        AddProduct(db, 2, 10, "KEDR ручка 42-323-CP-R");
        AddProduct(db, 3, 10, "KEDR ручка 42-323-AB-L No name");
        await db.SaveChangesAsync();
        return db;
    }

    private static readonly string FirstFile = Csv(
        Row(1, "KEDR ручка 42-323", "AB", "L"),
        Row(2, "KEDR ручка 42-323", "CR", "R", reasonCode: "unresolved_material_code"),
        Row(3, "KEDR ручка 42-323", "AB", "L", noName: true));

    [Fact]
    public async Task DryRun_ReportsButWritesNothing()
    {
        await using var db = await SeededAsync();

        var report = await RunAsync(db, Dictionary, FirstFile, apply: false);

        Assert.False(report.Applied);
        Assert.Empty(report.Errors);
        Assert.Equal(3, report.ProductsInFile);
        Assert.True(report.ProductValues.Created > 0);
        Assert.Equal(0, await db.Attributes.CountAsync());
        Assert.Equal(0, await db.ProductAttributeValues.CountAsync());
        Assert.Equal(0, await db.ProductAttributeReviews.CountAsync());
    }

    [Fact]
    public async Task Apply_StoresValuesReviewsAndCategoryFilters_AndSecondRunChangesNothing()
    {
        await using var db = await SeededAsync();
        var namesBefore = await db.Products.AsNoTracking().OrderBy(p => p.Id).Select(p => p.Name).ToListAsync();

        var first = await RunAsync(db, Dictionary, FirstFile, apply: true);
        var second = await RunAsync(db, Dictionary, FirstFile, apply: true);

        Assert.Empty(first.Errors);
        var colorId = (await db.Attributes.SingleAsync(a => a.Code == "color")).Id;
        Assert.Equal("kolir", (await db.Attributes.SingleAsync(a => a.Id == colorId)).Slug);
        Assert.Equal("antychna-bronza", (await db.AttributeValues.SingleAsync(v => v.Code == "AB")).Slug);

        var chrome = await db.ProductAttributeValues.SingleAsync(v => v.ProductId == ProductId.From(2) && v.AttributeId == colorId);
        Assert.False(chrome.IsConfirmed); // CP → CR was decoded by an assistant, not confirmed by the owner.
        Assert.Equal(AttributeValueSource.Import, chrome.Source);

        var reviews = await db.ProductAttributeReviews.ToDictionaryAsync(r => r.ProductId.Value);
        Assert.Equal(AttributeReviewState.Clear, reviews[1].State);
        Assert.Equal(AttributeReviewState.NeedsReview, reviews[2].State);
        Assert.Equal("Код, не розшифровано", reviews[2].Reasons.Single().Text);
        Assert.Equal(AttributeReviewReason.NoName, reviews[3].Reasons.Single().Code);
        Assert.Equal("KEDR ручка 42-323", reviews[1].BaseName);

        var undecoded = await db.AttributeAliases.SingleAsync(a => a.RawValue == "AL");
        Assert.Null(undecoded.ValueId);

        // Two colors and two sides in category 10 → both offered as filters.
        Assert.Equal(2, await db.CategoryAttributes.CountAsync());

        Assert.Equal(0, second.Attributes.Created + second.Attributes.Updated);
        Assert.Equal(0, second.Values.Created + second.Values.Updated);
        Assert.Equal(0, second.Aliases.Created + second.Aliases.Updated);
        Assert.Equal(0, second.ProductValues.Created + second.ProductValues.Updated + second.ProductValues.Removed);
        Assert.Equal(0, second.Reviews.Created + second.Reviews.Updated);
        Assert.Equal(0, second.CategoryAttributes.Created);

        var namesAfter = await db.Products.AsNoTracking().OrderBy(p => p.Id).Select(p => p.Name).ToListAsync();
        Assert.Equal(namesBefore, namesAfter);
    }

    [Fact]
    public async Task Reimport_KeepsManualValues_AcceptedReviews_AndRemovesOnlyStaleAutomaticValues()
    {
        await using var db = await SeededAsync();
        await RunAsync(db, Dictionary, FirstFile, apply: true);

        var color = await db.Attributes.SingleAsync(a => a.Code == "color");
        var chromeId = (await db.AttributeValues.SingleAsync(v => v.Code == "CR")).Id;
        var manual = await db.ProductAttributeValues.SingleAsync(v => v.ProductId == ProductId.From(1) && v.AttributeId == color.Id);
        manual.SetManually(chromeId, null, DateTimeOffset.UtcNow);
        var review = await db.ProductAttributeReviews.SingleAsync(r => r.ProductId == ProductId.From(2));
        review.Accept("admin@classkedr.com.ua", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // The new file drops the side of product 2 and still sees a problem in it.
        var secondFile = Csv(
            Row(1, "KEDR ручка 42-323", "AB", "L"),
            Row(2, "KEDR ручка 42-323", "CR", "null", reasonCode: "unresolved_material_code"),
            Row(3, "KEDR ручка 42-323", "AB", "L", noName: true));
        var report = await RunAsync(db, Dictionary, secondFile, apply: true);

        Assert.Equal(1, report.ProductValues.Skipped);
        Assert.Equal(1, report.ProductValues.Removed);
        var kept = await db.ProductAttributeValues.SingleAsync(v => v.ProductId == ProductId.From(1) && v.AttributeId == color.Id);
        Assert.Equal(chromeId, kept.ValueId);
        Assert.Equal(AttributeValueSource.Manual, kept.Source);
        Assert.True(kept.IsLocked);
        var side = await db.Attributes.SingleAsync(a => a.Code == "side");
        Assert.False(await db.ProductAttributeValues.AnyAsync(v => v.ProductId == ProductId.From(2) && v.AttributeId == side.Id));
        Assert.Equal(AttributeReviewState.Accepted,
            (await db.ProductAttributeReviews.SingleAsync(r => r.ProductId == ProductId.From(2))).State);
    }

    [Fact]
    public async Task UnknownProduct_IsReportedAndTheRestIsImported()
    {
        await using var db = await SeededAsync();

        var report = await RunAsync(db, Dictionary, Csv(Row(1, "Ручка", "AB", "L"), Row(999, "Ручка", "AB", "R")),
            apply: true);

        Assert.Contains(report.Errors, error => error.Contains("product 999", StringComparison.Ordinal));
        Assert.Equal(1, await db.ProductAttributeReviews.CountAsync());
    }

    [Fact]
    public async Task RealFiles_ImportWithoutErrors_AndSecondRunChangesNothing()
    {
        var csvPath = RepositoryFile("docs/catalog-attributes/products_attributes.csv");
        var dictionaryJson = await File.ReadAllTextAsync(RepositoryFile("docs/catalog-attributes/dictionaries.json"));
        var csv = await File.ReadAllTextAsync(csvPath);

        await using var db = CreateContext();
        var rows = global::Catalog.Application.Attributes.Import.AttributeCsvFile.Read(new StringReader(csv)).Products;
        var lines = csv.Split('\n');
        foreach (var categoryId in rows.Select(row => CategoryOf(lines, row.LineNumber)).Distinct())
            AddCategory(db, categoryId);
        foreach (var row in rows)
            AddProduct(db, row.ProductId, CategoryOf(lines, row.LineNumber), $"Product {row.ProductId}");
        await db.SaveChangesAsync();

        var first = await RunAsync(db, dictionaryJson, csv, apply: true);
        var second = await RunAsync(db, dictionaryJson, csv, apply: true);

        output.WriteLine($"attributes {first.Attributes}; values {first.Values}; aliases {first.Aliases}");
        output.WriteLine($"product values {first.ProductValues}; reviews {first.Reviews}; " +
                         $"category filters {first.CategoryAttributes}; need review {first.ProductsNeedingReview}");
        Assert.Empty(first.Errors);
        Assert.Equal(2330, first.ProductsInFile);
        Assert.Equal(2330, first.Reviews.Created);
        Assert.True(first.ProductsNeedingReview >= 1629);
        Assert.Equal(0, second.ProductValues.Created + second.ProductValues.Updated + second.ProductValues.Removed);
        Assert.Equal(0, second.Reviews.Created + second.Reviews.Updated);
        Assert.Equal(0, second.Values.Created + second.Aliases.Created + second.CategoryAttributes.Created);

        output.WriteLine($"groups {first.Groups.Groups}; members {first.Groups.Members}; same variants groups " +
                         $"{first.Groups.GroupsWithSameVariants}, products {first.Groups.ProductsWithSameVariants}");
        // 395 base-name groups (1531 products) and 763 one-product proposals for the other on-site products
        // (spec catalog/007 R-02); the 36 No name products stay outside.
        Assert.Equal(395 + 763, first.Groups.Groups.Created);
        Assert.Equal(1531 + 763, first.Groups.Members.Created);
        Assert.Equal(33, first.Groups.GroupsWithSameVariants);
        Assert.Equal(0, second.Groups.Groups.Created + second.Groups.Groups.Updated + second.Groups.Groups.Removed);
        Assert.Equal(0, second.Groups.Members.Created + second.Groups.Members.Updated + second.Groups.Members.Removed);
        Assert.Equal(0, second.Groups.ReviewsChanged);
    }

    private static int CategoryOf(string[] lines, int lineNumber)
        => int.Parse(lines[lineNumber - 1].Split(',')[1], System.Globalization.CultureInfo.InvariantCulture);
}
