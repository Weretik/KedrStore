using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using global::Catalog.Application.Attributes.Import;
using global::Catalog.Application.Jobs.Attributes;
using global::Catalog.Application.Features.Attributes.Common;
using global::Catalog.Application.Features.Attributes.DecideCode;
using global::Catalog.Application.Features.Attributes.GetCodeProducts;
using global::Catalog.Application.Features.ProductAttributes.Common;
using global::Catalog.Application.Features.ProductAttributes.Set;
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
using global::Catalog.Infrastructure.DataBase;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Attributes;

/// <summary>
/// Spec catalog/006 phase 4: the .NET recognizer over the real names, categories, and dictionary stores the same
/// values as the CSV import, keeps a person's values, and applies the owner's code decisions to the products.
/// </summary>
public sealed class AttributeRecognitionTests(ITestOutputHelper output)
{
    /// <summary>Ancestor categories that are not in the CSV get ids from here.</summary>
    private const int FirstAncestorId = 9_000_000;

    /// <summary>The real catalog: category tree from the CSV paths, original names, then the CSV import.</summary>
    private static async Task<CatalogDbContext> ImportedRealCatalogAsync()
    {
        var csv = await File.ReadAllTextAsync(RepositoryFile("docs/catalog-attributes/products_attributes.csv"));
        List<Dictionary<string, string>> rows;
        using (var reader = new StringReader(csv))
        {
            var records = CsvText.ReadRecords(reader).ToList();
            var header = records[0].Fields.Select(cell => cell.TrimStart('\uFEFF')).ToList();
            rows = records.Skip(1).Where(record => record.Fields.Count == header.Count)
                .Select(record => header.Zip(record.Fields).ToDictionary(pair => pair.First, pair => pair.Second))
                .ToList();
        }

        var db = CreateContext();
        var idByPath = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var leaf in rows.GroupBy(row => int.Parse(row["category_id"])).Select(group => group.First()))
            idByPath[leaf["category_path"]] = int.Parse(leaf["category_id"]);

        foreach (var path in idByPath.Keys.ToList())
        {
            var parts = path.Split(" > ");
            for (var length = 1; length < parts.Length; length++)
            {
                var ancestor = string.Join(" > ", parts.Take(length));
                if (!idByPath.ContainsKey(ancestor))
                    idByPath[ancestor] = FirstAncestorId + idByPath.Count;
            }
        }

        foreach (var (path, id) in idByPath.OrderBy(pair => pair.Key.Length))
        {
            var separator = path.LastIndexOf(" > ", StringComparison.Ordinal);
            ProductCategoryId? parentId = separator < 0 ? null : ProductCategoryId.From(idByPath[path[..separator]]);
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(id), Root, path[(separator < 0 ? 0 : separator + 3)..],
                $"katehoriia-{id}", CategoryPath.From($"n{id}"), parentId));
        }

        foreach (var row in rows)
            AddProduct(db, int.Parse(row["product_id"]), int.Parse(row["category_id"]), row["original_name"]);
        await db.SaveChangesAsync();

        var dictionary = await File.ReadAllTextAsync(RepositoryFile("docs/catalog-attributes/dictionaries.json"));
        Assert.Empty((await RunAsync(db, dictionary, csv, apply: true)).Errors);
        return db;
    }

    private static async Task<AttributeImportReport> RecognizeAsync(CatalogDbContext db, bool apply,
        IReadOnlyCollection<int>? productIds = null)
    {
        var report = await RecognitionJob(db).RunAsync(apply, productIds, CancellationToken.None);
        db.ChangeTracker.Clear();
        return report;
    }

    private static int Changes(AttributeImportReport report)
        => report.ProductValues.Created + report.ProductValues.Updated + report.ProductValues.Removed
           + report.Reviews.Created + report.Reviews.Updated;

    private static async Task<List<(string Attribute, int? Value, decimal? Number)>> ValuesAsync(CatalogDbContext db,
        int productId)
        => (await db.ProductAttributeValues.AsNoTracking()
                .Where(value => value.ProductId == ProductId.From(productId))
                .Join(db.Attributes, value => value.AttributeId, attribute => attribute.Id,
                    (value, attribute) => new { attribute.Code, value.ValueId, value.NumberValue, value.Position })
                .ToListAsync())
            .OrderBy(value => value.Code, StringComparer.Ordinal).ThenBy(value => value.Position)
            .Select(value => (value.Code, value.ValueId?.Value, value.NumberValue))
            .ToList();

    [Fact]
    public async Task RealCatalog_RecognizedAgain_GivesTheImportedValues()
    {
        await using var db = await ImportedRealCatalogAsync();

        var dryRun = await RecognizeAsync(db, apply: false);
        var applied = await RecognizeAsync(db, apply: true);
        var again = await RecognizeAsync(db, apply: true);

        output.WriteLine($"dry run: product values {dryRun.ProductValues}; reviews {dryRun.Reviews}; values {dryRun.Values}");
        output.WriteLine($"applied: product values {applied.ProductValues}; reviews {applied.Reviews}");
        Assert.Empty(dryRun.Errors);
        Assert.Equal(2330, dryRun.ProductsInFile);
        Assert.Equal(0, dryRun.Values.Created);
        Assert.Equal(0, again.ProductValues.Created + again.ProductValues.Updated + again.ProductValues.Removed);
        Assert.Equal(0, again.Reviews.Created + again.Reviews.Updated);
        Assert.Equal(0, again.Groups.Groups.Created + again.Groups.Groups.Removed + again.Groups.Members.Removed);
        Assert.All(await db.ProductAttributeValues.AsNoTracking().Where(value => !value.IsLocked).ToListAsync(),
            value => Assert.Equal(AttributeValueSource.Parser, value.Source));
    }

    [Fact]
    public async Task NewProduct_GetsParserValues_AndAPersonsValueIsKept()
    {
        await using var db = await ImportedRealCatalogAsync();
        var original = await db.Products.AsNoTracking().SingleAsync(product => product.Id == ProductId.From(60));
        AddProduct(db, 999_001, original.CategoryId.Value, original.Name);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var color = await db.Attributes.AsNoTracking().SingleAsync(attribute => attribute.Code == "color");
        var otherColor = await db.AttributeValues.AsNoTracking()
            .SingleAsync(value => value.AttributeId == color.Id && value.Code == "AB");
        var set = new SetProductAttributeCommandHandler(db, new TestRepository<ProductAttributeValue>(db),
            GroupBuilder(db), new ProductAttributesReader(db), TimeProvider.System);
        Assert.True((await set.Handle(new SetProductAttributeCommand(60, color.Id.Value,
                new SetProductAttributeRequest([new SetProductAttributeValueRequest(otherColor.Id.Value, null)])),
            CancellationToken.None)).IsSuccess);
        db.ChangeTracker.Clear();
        var manual = await ValuesAsync(db, 60);

        var report = await RecognizeAsync(db, apply: true, [60, 999_001]);

        Assert.Empty(report.Errors);
        Assert.Equal(manual, await ValuesAsync(db, 60));
        var copy = await ValuesAsync(db, 999_001);
        Assert.NotEmpty(copy);
        Assert.All(await db.ProductAttributeValues.AsNoTracking()
            .Where(value => value.ProductId == ProductId.From(999_001)).ToListAsync(),
            value => Assert.Equal(AttributeValueSource.Parser, value.Source));
        Assert.NotNull(await db.ProductAttributeReviews.AsNoTracking()
            .SingleOrDefaultAsync(review => review.ProductId == ProductId.From(999_001)));
    }

    [Fact]
    public async Task OwnerDecidesACode_AsMeaningNothing_TheProductLosesThatReason()
    {
        await using var db = await ImportedRealCatalogAsync();
        var codes = new UnresolvedCodeReader(db, new CodeMeaningReader(db));
        var al = (await codes.ListAsync(CancellationToken.None))
            .First(code => code.RawValue == "AL" && code.ValueId is null);
        var before = await ReviewTextAsync(db, 60);
        Assert.Contains("AL", before);

        var decide = new DecideAttributeCodeCommandHandler(db, new TestRepository<AttributeAlias>(db),
            new TestRepository<AttributeAliasCategoryValue>(db), new TestRepository<AttributeAliasMeaning>(db), codes,
            RecognitionJob(db), TimeProvider.System, NullLogger<DecideAttributeCodeCommandHandler>.Instance);
        var result = await decide.Handle(new DecideAttributeCodeCommand(al.AliasId, new DecideAttributeCodeRequest(null)),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        Assert.True(result.IsSuccess);
        var after = await ReviewTextAsync(db, 60);
        output.WriteLine($"before: {before}\nafter: {after}");
        Assert.DoesNotContain("«AL»", after);
        Assert.Equal(0, Changes(await RecognizeAsync(db, apply: true, [60])));
    }

    [Fact]
    public async Task OwnerDecidesACode_ForOneCategory_OnlyThatCategoryLosesTheReason_AndTheProductsAreListed()
    {
        await using var db = await ImportedRealCatalogAsync();
        var codes = new UnresolvedCodeReader(db, new CodeMeaningReader(db));
        var al = (await codes.ListAsync(CancellationToken.None))
            .First(code => code.RawValue == "AL" && code.ValueId is null);
        // 60 is a handle «На планці» (5915), 80 a handle «На розетці (HRoz-06/HRoz-07)» (5854); both contain «AL».
        Assert.Contains("«AL»", await ReviewTextAsync(db, 60));
        Assert.Contains("«AL»", await ReviewTextAsync(db, 80));

        var listed = await new GetCodeProductsQueryHandler(db, new CodeMeaningReader(db)).Handle(new GetCodeProductsQuery(al.AliasId),
            CancellationToken.None);
        var handles = Assert.Single(listed.Value.Categories, category => category.CategoryId == 5915);
        Assert.Contains(handles.Products, product => product.ProductId == 60);
        Assert.False(handles.HasOwnValue);

        var decide = new DecideAttributeCodeCommandHandler(db, new TestRepository<AttributeAlias>(db),
            new TestRepository<AttributeAliasCategoryValue>(db), new TestRepository<AttributeAliasMeaning>(db), codes,
            RecognitionJob(db), TimeProvider.System, NullLogger<DecideAttributeCodeCommandHandler>.Instance);
        var result = await decide.Handle(new DecideAttributeCodeCommand(al.AliasId,
                new DecideAttributeCodeRequest(null, ApplyToAll: false,
                    [new DecideAttributeCodeCategoryRequest(5915, null)])),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Code.ValueId);
        var alias = await db.AttributeAliases.AsNoTracking()
            .SingleAsync(item => item.Id == AttributeAliasId.Create(al.AliasId));
        Assert.False(alias.IsEditedManually);
        Assert.DoesNotContain("«AL»", await ReviewTextAsync(db, 60));
        Assert.Contains("«AL»", await ReviewTextAsync(db, 80));
        var after = await new GetCodeProductsQueryHandler(db, new CodeMeaningReader(db)).Handle(new GetCodeProductsQuery(al.AliasId),
            CancellationToken.None);
        Assert.True(Assert.Single(after.Value.Categories, category => category.CategoryId == 5915).HasOwnValue);

        // An empty list removes the category's own decision again.
        Assert.True((await decide.Handle(new DecideAttributeCodeCommand(al.AliasId,
            new DecideAttributeCodeRequest(null, ApplyToAll: false, [])), CancellationToken.None)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Contains("«AL»", await ReviewTextAsync(db, 60));
    }

    [Fact]
    public async Task OwnerGivesACompositeCode_AValueOfAnotherAttribute_TheProductsGetIt_ExceptACategoryWithItsOwnDecoding()
    {
        await using var db = await ImportedRealCatalogAsync();
        var codes = new UnresolvedCodeReader(db, new CodeMeaningReader(db));
        var zcg = (await codes.ListAsync(CancellationToken.None)).First(code => code.RawValue == "ZCG");
        var color = await db.Attributes.AsNoTracking().SingleAsync(attribute => attribute.Code == "color");
        var gold = await db.AttributeValues.AsNoTracking()
            .SingleAsync(value => value.AttributeId == color.Id && value.Code == "G");
        var products = (await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(product => new { product.Id, product.CategoryId, product.Name })
                .ToListAsync())
            .Where(product => global::Catalog.Application.Attributes.Recognition.ProductAttributeRecognizer
                .Mentions(product.Name, "ZCG"))
            .ToList();
        // 7894 is a cylinder (2680) and 7158 a hinge (6139) of the real catalog; the hinge's ZCG is not a cylinder code.
        const int cylinder = 7894, hinge = 7158, cylinders = 2680;
        Assert.Contains(products, product => product.Id.Value == cylinder && product.CategoryId.Value == cylinders);
        Assert.Contains(products, product => product.Id.Value == hinge);
        output.WriteLine(await ReviewTextAsync(db, cylinder));
        Assert.Contains("ZCG", await ReviewTextAsync(db, cylinder));

        var decide = new DecideAttributeCodeCommandHandler(db, new TestRepository<AttributeAlias>(db),
            new TestRepository<AttributeAliasCategoryValue>(db), new TestRepository<AttributeAliasMeaning>(db), codes,
            RecognitionJob(db), TimeProvider.System, NullLogger<DecideAttributeCodeCommandHandler>.Instance);
        var result = await decide.Handle(new DecideAttributeCodeCommand(zcg.AliasId,
                new DecideAttributeCodeRequest(null, Meanings: [new CodeMeaningRequest(color.Id.Value, gold.Id.Value)])),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        Assert.True(result.IsSuccess);
        var meaning = Assert.Single(result.Value.Code.Meanings);
        Assert.Equal((color.Id.Value, gold.Id.Value), (meaning.AttributeId, meaning.ValueId));
        Assert.DoesNotContain("«ZCG»", await ReviewTextAsync(db, cylinder));
        Assert.True(await HasGoldAsync(cylinder));
        Assert.False(await HasGoldAsync(hinge));
        Assert.Equal(0, Changes(await RecognizeAsync(db, apply: true, [cylinder, hinge])));

        // The cylinders get their own decoding without a colour: the meaning for all products no longer reaches them.
        Assert.True((await decide.Handle(new DecideAttributeCodeCommand(zcg.AliasId,
                new DecideAttributeCodeRequest(null, ApplyToAll: false,
                    [new DecideAttributeCodeCategoryRequest(cylinders, null, [])])),
            CancellationToken.None)).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.False(await HasGoldAsync(cylinder));
        var listed = await new GetCodeProductsQueryHandler(db, new CodeMeaningReader(db))
            .Handle(new GetCodeProductsQuery(zcg.AliasId), CancellationToken.None);
        var own = Assert.Single(listed.Value.Categories, category => category.CategoryId == cylinders);
        Assert.True(own.HasOwnValue);
        Assert.Empty(own.Meanings);

        // A value must belong to the attribute it is given for.
        Assert.False((await decide.Handle(new DecideAttributeCodeCommand(zcg.AliasId,
            new DecideAttributeCodeRequest(null, Meanings: [new CodeMeaningRequest(zcg.AttributeId, gold.Id.Value)])),
            CancellationToken.None)).IsSuccess);

        Task<bool> HasGoldAsync(int productId)
            => db.ProductAttributeValues.AsNoTracking().AnyAsync(value => value.ProductId == ProductId.From(productId)
                && value.AttributeId == color.Id && value.ValueId == gold.Id && value.IsConfirmed);
    }

    private static async Task<string> ReviewTextAsync(CatalogDbContext db, int productId)
        => string.Join(" | ", (await db.ProductAttributeReviews.AsNoTracking()
                .SingleAsync(review => review.ProductId == ProductId.From(productId)))
            .Reasons.Select(reason => $"{reason.Code}: {reason.Text}"));
}
