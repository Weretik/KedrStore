using System.Security.Claims;
using BuildingBlocks.Application.Users;
using BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using IntegrationTests.Platform.Caching;
using global::Catalog.Application.Features.CategoryPlacement.Commands.AddCategorySiteModels;
using global::Catalog.Application.Features.Groups.Shared;
using global::Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;
using global::Catalog.Application.Features.SiteModels.Commands.AddSiteModelProducts;
using global::Catalog.Application.Features.SiteModels.Commands.ChangeSiteModelCategory;
using global::Catalog.Application.Features.SiteModels.Queries.GetSiteModelCategories;
using global::Catalog.Application.Features.SiteModels.Queries.GetSiteModels;
using global::Catalog.Application.Features.SiteModels.Shared;
using global::Catalog.Application.Features.SiteModels.Shared.State;
using global::Catalog.Contracts.SiteModels;
using global::Catalog.Domain.Categories;
using global::Catalog.Domain.Groups;
using global::Catalog.Domain.Groups.Members;
using global::Catalog.Domain.Products;
using global::Catalog.Infrastructure.Persistence;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.SiteModels;

/// <summary>Spec catalog/007 stage 9: a model's own category and products of any 1C category (R-22–R-25).</summary>
public sealed class SiteModelCategoryTests
{
    private const int Parent = 10;
    private const int Final = 11;
    private const int OtherFinal = 12;

    /// <summary>Category 10 is a parent of the final categories 11 and 12; products go to 11 unless listed.</summary>
    private static async Task<CatalogDbContext> CatalogAsync(int[] products, int[]? inOtherCategory = null)
    {
        var db = CreateContext();
        AddCategory(db, Parent);
        foreach (var child in new[] { Final, OtherFinal })
        {
            var category = ProductCategory.Create(ProductCategoryId.Create(child), Root, $"Категорія {child}",
                $"katehoriia-{child}", CategoryPath.Create($"n{Parent}.n{child}"), ProductCategoryId.Create(Parent)).Value;
            category.SetShownOnSite(true, DateTimeOffset.UtcNow);
            db.Categories.Add(category);
        }
        foreach (var id in products)
            AddProduct(db, id, inOtherCategory?.Contains(id) == true ? OtherFinal : Final, $"Товар {id}");
        await db.SaveChangesAsync();
        return db;
    }

    private sealed class Handlers(CatalogDbContext db)
    {
        private SiteModelStateReader State { get; } = new(db, NoReadCache.Instance);
        private SiteModelReader Reader => new(State);

        private SiteModelChanges Changes => new(db, new TestRepository<ProductGroup>(db),
            new ProductGroupPins(db, new TestRepository<ProductGroupMember>(db)), GroupBuilder(db), State);

        public ChangeSiteModelCategoryHandler ChangeCategory => new(db, new TestRepository<ProductGroup>(db),
            new Manager(), Changes, Reader, TimeProvider.System, new CatalogTransaction(db));

        public AcceptSiteModelsHandler Accept => new(db, new Manager(),
            Changes, State, TimeProvider.System, new CatalogTransaction(db));

        public AddSiteModelProductsHandler Add => new(db, Changes, Reader, TimeProvider.System);

        public AddCategorySiteModelsHandler AddToCategory => new(db, new TestRepository<ProductGroup>(db),
            new Manager(), Changes, TimeProvider.System, new CatalogTransaction(db));

        public async Task<Ardalis.Result.Result<SiteModelDto>> MoveAsync(int modelId, int categoryId)
            => await ChangeCategory.Handle(new ChangeSiteModelCategoryCommand(modelId,
                new ChangeSiteModelCategoryRequest { CategoryId = categoryId }), CancellationToken.None);

        public async Task<SiteModelListDto> ListAsync(GetSiteModelsRequest request)
            => (await new GetSiteModelsHandler(State).Handle(new GetSiteModelsQuery(request),
                CancellationToken.None)).Value;

        public async Task<List<SiteModelCategoryDto>> CategoriesAsync()
            => (await new GetSiteModelCategoriesHandler(State).Handle(new GetSiteModelCategoriesQuery(),
                CancellationToken.None)).Value;
    }

    private static async Task<ProductGroup> ModelOfAsync(CatalogDbContext db, int productId)
    {
        var member = await db.ProductGroupMembers.AsNoTracking()
            .SingleAsync(row => row.ProductId == ProductId.Create(productId));
        return await db.ProductGroups.AsNoTracking().SingleAsync(group => group.Id == member.GroupId);
    }

    private static string Csv(params string[] rows) => CsvHeader + "\n" + string.Join("\n", rows) + "\n";

    [Fact]
    public async Task ProposedModel_MovedToAnotherCategory_IsAccepted_AndTheBuildLeavesIt_SC_M17()
    {
        await using var db = await CatalogAsync([1, 2]);
        var csv = Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L"));
        await RunAsync(db, Dictionary, csv, apply: true);
        var proposal = await ModelOfAsync(db, 1);
        Assert.Equal(ProductGroupStatus.Proposed, proposal.Status);

        var moved = await new Handlers(db).MoveAsync(proposal.Id.Value, OtherFinal);

        Assert.True(moved.IsSuccess);
        Assert.Equal(("accepted", OtherFinal), (moved.Value.Status, moved.Value.CategoryId));
        Assert.Equal([1, 2], moved.Value.Products.Select(product => product.ProductId).Order());
        Assert.Equal(["Категорія 11"], moved.Value.OneCCategories);
        Assert.True(await db.ProductGroupMembers.AsNoTracking()
            .Where(row => row.GroupId == proposal.Id).AllAsync(row => row.IsPinned));

        db.ChangeTracker.Clear();
        await RunAsync(db, Dictionary, csv, apply: true);
        var after = await ModelOfAsync(db, 1);
        Assert.Equal((proposal.Id, ProductCategoryId.Create(OtherFinal)), (after.Id, after.CategoryId));
        Assert.Equal(proposal.Id, (await ModelOfAsync(db, 2)).Id);
        // Each product keeps its own 1C category.
        Assert.True(await db.Products.AsNoTracking().Where(product => product.Id == ProductId.Create(1))
            .AllAsync(product => product.CategoryId == ProductCategoryId.Create(Final)));
    }

    [Fact]
    public async Task ParentCategory_IsRefused_UnknownModelIsNotFound_SameCategoryChangesNothing_SC_M18()
    {
        await using var db = await CatalogAsync([1, 2]);
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L")), apply: true);
        var model = await ModelOfAsync(db, 1);
        var handlers = new Handlers(db);

        var parent = await handlers.MoveAsync(model.Id.Value, Parent);
        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, parent.Status);
        Assert.Contains(parent.ValidationErrors, error => error.ErrorCode == "Catalog.SiteModel.CategoryNotFinal");
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, (await handlers.MoveAsync(999, OtherFinal)).Status);

        var same = await handlers.MoveAsync(model.Id.Value, Final);
        Assert.True(same.IsSuccess);
        Assert.Equal(Final, same.Value.CategoryId);
        Assert.Empty(same.Value.OneCCategories);
    }

    [Fact]
    public async Task ProductOfAnotherCategory_JoinsAModel_AndIsCountedUnderTheModelsCategory_SC_M19()
    {
        await using var db = await CatalogAsync([1, 2, 3], inOtherCategory: [3]);
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L"),
            Row(3, "Ручка", "AB", "L")), apply: true);
        var hinges = await ModelOfAsync(db, 1);
        var handlers = new Handlers(db);
        await handlers.Accept.Handle(new AcceptSiteModelsCommand(new AcceptSiteModelsRequest { Ids = [hinges.Id.Value] }),
            CancellationToken.None);

        var added = await handlers.Add.Handle(new AddSiteModelProductsCommand(hinges.Id.Value,
            new AddSiteModelProductsRequest { ProductIds = [3] }), CancellationToken.None);

        Assert.True(added.IsSuccess);
        Assert.Equal([1, 2, 3], added.Value.Products.Select(product => product.ProductId).Order());
        Assert.Equal(["Категорія 12"], added.Value.OneCCategories);
        db.ChangeTracker.Clear();

        var row = Assert.Single((await handlers.ListAsync(new GetSiteModelsRequest())).Items);
        Assert.Equal((hinges.Id.Value, Final), (row.Id, row.CategoryId));
        Assert.Equal("Категорія 10 > Категорія 11", row.CategoryName);
        Assert.Equal("Категорія 10 > Категорія 11", added.Value.CategoryName);
        Assert.Equal(["Категорія 12"], row.OneCCategories);

        var categories = await handlers.CategoriesAsync();
        Assert.Equal([Parent, Final], categories.Select(category => category.Id));
        Assert.Equal(3, categories.Single(category => category.Id == Final).OnSiteProducts);
    }

    [Fact]
    public async Task ModelsAreAddedToACategory_AProposedOneIsAccepted_AParentIsRefused_SC_MC03()
    {
        await using var db = await CatalogAsync([1, 2, 3]);
        await RunAsync(db, Dictionary, Csv(Row(1, "Завіса", "AB", "L"), Row(2, "Завіса", "CR", "L"),
            Row(3, "Ручка", "AB", "L")), apply: true);
        var hinges = await ModelOfAsync(db, 1);
        var handles = await ModelOfAsync(db, 3);
        var handlers = new Handlers(db);

        var parent = await handlers.AddToCategory.Handle(
            new AddCategorySiteModelsCommand(Parent, [hinges.Id.Value]), CancellationToken.None);
        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, parent.Status);
        var unknown = await handlers.AddToCategory.Handle(
            new AddCategorySiteModelsCommand(OtherFinal, [hinges.Id.Value, 999]), CancellationToken.None);
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, unknown.Status);

        var added = await handlers.AddToCategory.Handle(
            new AddCategorySiteModelsCommand(OtherFinal, [hinges.Id.Value, handles.Id.Value]), CancellationToken.None);

        Assert.True(added.IsSuccess);
        db.ChangeTracker.Clear();
        var moved = await db.ProductGroups.AsNoTracking()
            .Where(group => group.Id == hinges.Id || group.Id == handles.Id).ToListAsync();
        Assert.All(moved, model => Assert.Equal(ProductCategoryId.Create(OtherFinal), model.CategoryId));
        Assert.All(moved, model => Assert.Equal(ProductGroupStatus.Accepted, model.Status));
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
