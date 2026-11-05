using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Contracts;
using global::Catalog.Application.Features.Groups.Common;
using global::Catalog.Application.Features.SiteModels.Accept;
using global::Catalog.Application.Features.SiteModels.Common;
using global::Catalog.Application.Features.SiteModels.Create;
using global::Catalog.Application.Features.SiteModels.FillFromNames;
using global::Catalog.Application.Features.SiteModels.Get;
using global::Catalog.Application.Features.SiteModels.GetHealth;
using global::Catalog.Application.Features.SiteModels.GetList;
using global::Catalog.Application.Features.SiteModels.SetAxes;
using global::Catalog.Application.Features.SiteModels.SetTexts;
using global::Catalog.Application.Attributes.Import;
using global::Catalog.Application.Slugs;
using global::Catalog.Contracts.SiteModels;
using global::Catalog.Domain.Entities;
using global::Catalog.Domain.Entities.Attributes;
using global::Catalog.Domain.Entities.Groups;
using global::Catalog.Domain.Enums;
using global::Catalog.Domain.ValueObjects;
using global::Catalog.Infrastructure.DataBase;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.SiteModels;

/// <summary>Spec catalog/007 stage 2: proposed and accepted site models, axes, texts, and their health.</summary>
public sealed class SiteModelTests
{
    private const int Parent = 10;
    private const int Final = 11;
    private const int OtherFinal = 12;

    private static string Csv(params string[] rows) => CsvHeader + "\n" + string.Join("\n", rows) + "\n";

    /// <summary>Category 10 is a parent of the final categories 11 and 12; products go to 11 unless listed.</summary>
    private static async Task<CatalogDbContext> CatalogAsync(int[] products, int[]? inOtherCategory = null,
        int[]? offSite = null)
    {
        var db = CreateContext();
        AddCategory(db, Parent);
        foreach (var child in new[] { Final, OtherFinal })
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(child), Root, $"Категорія {child}",
                $"katehoriia-{child}", CategoryPath.From($"n{Parent}.n{child}"), ProductCategoryId.From(Parent)));
        foreach (var id in products)
            AddProduct(db, id, inOtherCategory?.Contains(id) == true ? OtherFinal : Final, $"Товар {id}");
        await db.SaveChangesAsync();

        foreach (var id in offSite ?? [])
        {
            var product = await db.Products.SingleAsync(item => item.Id == ProductId.From(id));
            product.UpdateFromOneC(product.Name, product.CategoryId, 1, exportToSite: false, false, false,
                DateTimeOffset.UtcNow);
        }

        await db.SaveChangesAsync();
        return db;
    }

    private sealed class Handlers(CatalogDbContext db)
    {
        public SiteModelStateReader State { get; } = new(db);
        public SiteModelReader Reader => new(State);

        public SiteModelChanges Changes => new(db, new TestRepository<ProductGroupAxis>(db),
            new ProductGroupPins(db, new TestRepository<ProductGroupMember>(db)), GroupBuilder(db), State);

        public AcceptSiteModelsCommandHandler Accept => new(db, new TestRepository<ProductGroup>(db), new Manager(),
            Changes, State, TimeProvider.System);

        public CreateSiteModelCommandHandler Create => new(new TestRepository<ProductGroup>(db), new SlugService(db),
            Changes, Reader, TimeProvider.System);

        public SetSiteModelAxesCommandHandler SetAxes => new(db, new TestRepository<ProductGroupAxis>(db), Reader);

        public SetSiteModelTextCommandHandler SetText => new(db, new TestRepository<ProductGroupText>(db), Reader,
            TimeProvider.System);

        public FillSiteModelFromNamesCommandHandler Fill => new(db, new TestRepository<ProductAttributeValue>(db),
            State, Changes, Reader, TimeProvider.System);

        public async Task<SiteModelDto> GetAsync(int id)
            => (await new GetSiteModelQueryHandler(Reader).Handle(new GetSiteModelQuery(id), CancellationToken.None))
                .Value;

        public async Task<SiteModelHealthDto> HealthAsync()
            => (await new GetSiteModelHealthQueryHandler(State).Handle(new GetSiteModelHealthQuery(),
                CancellationToken.None)).Value;

        public async Task<SiteModelListDto> ListAsync(GetSiteModelsRequest request)
            => (await new GetSiteModelsQueryHandler(State).Handle(new GetSiteModelsQuery(request),
                CancellationToken.None)).Value;
    }

    private static async Task<ProductGroup> ModelOfAsync(CatalogDbContext db, int productId)
    {
        var member = await db.ProductGroupMembers.AsNoTracking().SingleAsync(row => row.ProductId == ProductId.From(productId));
        return await db.ProductGroups.AsNoTracking().SingleAsync(group => group.Id == member.GroupId);
    }

    private static async Task<List<int>> ProductsOfAsync(CatalogDbContext db, ProductGroupId modelId)
        => await db.ProductGroupMembers.AsNoTracking()
            .Where(row => row.GroupId == modelId)
            .OrderBy(row => row.ProductId)
            .Select(row => row.ProductId.Value)
            .ToListAsync();

    [Fact]
    public async Task Build_ProposesModelsOfSeveralAndOfOne_OnlyForOnSiteProducts_SC_M01()
    {
        await using var db = await CatalogAsync([1, 2, 3, 4], offSite: [4]);
        var csv = Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L"), Row(3, "Ручка", "AB", "L"),
            Row(4, "Засувка", "AB", "L"));

        await RunAsync(db, Dictionary, csv, apply: true);

        var models = await db.ProductGroups.AsNoTracking().OrderBy(group => group.Name).ToListAsync();
        Assert.Equal(["Завіса", "Ручка"], models.Select(model => model.Name));
        Assert.All(models, model => Assert.Equal(ProductGroupStatus.Proposed, model.Status));
        Assert.Equal([1, 2], await ProductsOfAsync(db, models[0].Id));
        Assert.Equal([3], await ProductsOfAsync(db, models[1].Id));
        Assert.False(await db.ProductGroupMembers.AnyAsync(row => row.ProductId == ProductId.From(4)));

        var second = await RunAsync(db, Dictionary, csv, apply: true);
        Assert.Equal(0, second.Groups.Groups.Created + second.Groups.Groups.Updated + second.Groups.Groups.Removed);
        Assert.Equal(0, second.Groups.Members.Created + second.Groups.Members.Updated + second.Groups.Members.Removed);
    }

    [Fact]
    public async Task AcceptedModel_StaysAsItIs_WhenBaseNamesChange_SC_M02()
    {
        await using var db = await CatalogAsync([1, 2, 3]);
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L"),
            Row(3, "Ручка", "AB", "L")), apply: true);
        var model = await ModelOfAsync(db, 1);
        var handlers = new Handlers(db);

        var health = await handlers.Accept.Handle(
            new AcceptSiteModelsCommand(new AcceptSiteModelsRequest { Ids = [model.Id.Value] }), CancellationToken.None);
        Assert.True(health.IsSuccess);
        Assert.Equal(1, health.Value.ProductsWithoutModel);
        Assert.Equal(1, health.Value.ProposedModels);

        // Product 2 now reads as «Ручка»: the accepted model keeps it, its name, and its status.
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Ручка", "CR", "L"),
            Row(3, "Ручка", "AB", "L")), apply: true);

        var accepted = await db.ProductGroups.AsNoTracking().SingleAsync(group => group.Id == model.Id);
        Assert.Equal(ProductGroupStatus.Accepted, accepted.Status);
        Assert.Equal("Завіса", accepted.Name);
        Assert.Equal("manager@example.com", accepted.AcceptedBy);
        Assert.Equal([1, 2], await ProductsOfAsync(db, model.Id));
        Assert.Equal([3], await ProductsOfAsync(db, (await ModelOfAsync(db, 3)).Id));
    }

    [Fact]
    public async Task PersonCreatesModel_InFinalCategory_WithProductsOfThatCategoryOnly_SC_M03()
    {
        await using var db = await CatalogAsync([1, 2, 3, 4, 5], inOtherCategory: [5]);
        await RunAsync(db, Dictionary, Csv(Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "CR", "L"),
            Row(3, "Засувка", "AB", "L"), Row(4, "Засувка", "CR", "L"), Row(5, "Ручка", "AB", "L")), apply: true);
        var handlers = new Handlers(db);
        var latches = await ModelOfAsync(db, 3);
        await handlers.Accept.Handle(new AcceptSiteModelsCommand(new AcceptSiteModelsRequest { Ids = [latches.Id.Value] }),
            CancellationToken.None);

        async Task<Ardalis.Result.Result<SiteModelDto>> CreateAsync(int category, params int[] productIds)
            => await handlers.Create.Handle(new CreateSiteModelCommand(new CreateSiteModelRequest
            {
                CategoryId = category, Name = "Ручка KEDR", ProductIds = productIds
            }), CancellationToken.None);

        Assert.False((await CreateAsync(Parent, 1)).IsSuccess);
        Assert.False((await CreateAsync(Final, 1, 5)).IsSuccess);
        Assert.False((await CreateAsync(Final, 1, 3)).IsSuccess);

        var created = await CreateAsync(Final, 1);
        Assert.True(created.IsSuccess);
        Assert.Equal("accepted", created.Value.Status);
        Assert.Equal([1], created.Value.Products.Select(product => product.ProductId));

        // The proposal «Ручка» gave product 1 up; product 2 stays there as a one-product proposal.
        var rest = await ModelOfAsync(db, 2);
        Assert.Equal(ProductGroupStatus.Proposed, rest.Status);
        Assert.Equal([2], await ProductsOfAsync(db, rest.Id));
    }

    [Fact]
    public async Task ProposedModel_OffersColorAxis_AcceptSavesIt_FillFromNamesSetsConfirmedColors_SC_M04()
    {
        await using var db = await CatalogAsync([1, 2]);
        // AB is a confirmed color; CR comes from CP, which nobody confirmed (spec catalog/006 D-03).
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L")), apply: true);
        var model = await ModelOfAsync(db, 1);
        var handlers = new Handlers(db);

        var proposed = await handlers.GetAsync(model.Id.Value);
        var offered = Assert.Single(proposed.Axes);
        Assert.Equal(AttributeImportCatalog.Color, offered.Code);
        Assert.False(offered.IsSaved);
        Assert.Equal("swatch", offered.Display);

        await handlers.Accept.Handle(new AcceptSiteModelsCommand(new AcceptSiteModelsRequest { Ids = [model.Id.Value] }),
            CancellationToken.None);
        Assert.True(Assert.Single((await handlers.GetAsync(model.Id.Value)).Axes).IsSaved);

        db.ChangeTracker.Clear();
        var filled = await handlers.Fill.Handle(new FillSiteModelFromNamesCommand(model.Id.Value), CancellationToken.None);

        var bronze = filled.Value.Products.Single(product => product.ProductId == 1).Values
            .Single(value => value.AttributeId == offered.AttributeId);
        Assert.True(bronze.IsManual);
        Assert.Equal("Антична бронза", bronze.Name);
        var chrome = filled.Value.Products.Single(product => product.ProductId == 2);
        Assert.Null(chrome.Values.Single(value => value.AttributeId == offered.AttributeId).ValueId);
        Assert.Contains(SiteModelProblems.MissingAxisValue, chrome.Problems);
    }

    [Fact]
    public async Task Health_CountsProblems_AndTextsAndAColorFixClearThem_SC_M05()
    {
        await using var db = await CatalogAsync([1, 2, 3]);
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "AB", "L"),
            Row(3, "Ручка", "AB", "L")), apply: true);
        var handlers = new Handlers(db);
        var hinges = await ModelOfAsync(db, 1);
        var handle = await ModelOfAsync(db, 3);

        var before = await handlers.HealthAsync();
        Assert.Equal(3, before.ProductsWithoutModel);
        Assert.Equal(2, before.ProposedModels);

        await handlers.Accept.Handle(new AcceptSiteModelsCommand(new AcceptSiteModelsRequest
        {
            Ids = [hinges.Id.Value, handle.Id.Value]
        }), CancellationToken.None);
        var accepted = await handlers.HealthAsync();
        Assert.Equal(0, accepted.ProductsWithoutModel);
        Assert.Equal(1, accepted.ModelsWithoutAxes);
        Assert.Equal(2, accepted.ModelsWithoutSeo);

        var color = await db.Attributes.AsNoTracking().SingleAsync(item => item.Code == AttributeImportCatalog.Color);
        await handlers.SetAxes.Handle(new SetSiteModelAxesCommand(hinges.Id.Value,
            new SetSiteModelAxesRequest { AttributeIds = [color.Id.Value] }), CancellationToken.None);
        var same = await handlers.HealthAsync();
        Assert.Equal(0, same.ModelsWithoutAxes);
        Assert.Equal(2, same.ProductsWithSameCombination);
        var problemList = await handlers.ListAsync(new GetSiteModelsRequest { Problem = SiteModelProblems.SameCombination });
        Assert.Equal([hinges.Id.Value], problemList.Items.Select(item => item.Id));

        foreach (var modelId in new[] { hinges.Id.Value, handle.Id.Value })
        foreach (var language in ProductGroupText.Languages)
        {
            var saved = await handlers.SetText.Handle(new SetSiteModelTextCommand(modelId, language,
                new SetSiteModelTextRequest { ShortDescription = "Опис", SeoDescription = "Опис для пошуку" }),
                CancellationToken.None);
            Assert.True(saved.IsSuccess);
        }

        var chrome = await db.AttributeValues.AsNoTracking()
            .SingleAsync(value => value.AttributeId == color.Id && value.Code == "CR");
        var row = await db.ProductAttributeValues
            .SingleAsync(value => value.ProductId == ProductId.From(2) && value.AttributeId == color.Id);
        row.SetManually(chrome.Id, null, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var fixedHealth = await handlers.HealthAsync();
        Assert.Equal(0, fixedHealth.ProductsWithSameCombination);
        Assert.Equal(0, fixedHealth.ModelsWithoutSeo);
        Assert.Equal(0, fixedHealth.ProductsWithoutModel);
        Assert.Equal(0, fixedHealth.ProposedModels);
    }

    private sealed class Manager : ICurrentUserService
    {
        public string UserId => "2";
        public bool IsAuthenticated => true;
        public string UserName => "manager";
        public string Email => "manager@example.com";
        public IEnumerable<string> Roles => ["Manager"];
        public IEnumerable<Claim> Claims => [];
        public bool IsInRole(string role) => role == "Manager";
        public bool HasPermission(string permission) => false;
        public string GetClaimValue(string claimType) => string.Empty;
    }
}
