using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using global::Catalog.Application.Features.Attributes.Shared.Import;
using global::Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;
using global::Catalog.Application.Features.SiteModels.Shared;
using global::Catalog.Application.Features.SiteModels.Shared.Public;
using global::Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;
using global::Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;
using global::Catalog.Application.Features.SiteModels.Queries.GetPublicProductModelAddress;
using global::Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;
using global::Catalog.Application.Features.OneCSync.Mapping;
using global::Catalog.Application.Features.Slugs.Shared;
using global::Catalog.Contracts.SiteModels;
using global::Catalog.Contracts.SiteModels.Public;
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
using global::Catalog.Infrastructure.Integrations.Html;
using global::Catalog.Infrastructure.Integrations.OneC;
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
using IntegrationTests.Catalog.ProductMedia;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.SiteModels;

/// <summary>Spec catalog/007 stage 5: the public model lists, filters, search, model page, and product addresses.</summary>
public sealed class PublicSiteModelTests
{
    private const int Parent = 10;
    private const int Final = 11;
    private const int OtherFinal = 12;

    /// <summary>
    /// Hinges 1 (left) and 2 (right, out of stock) form the accepted model «Завіса»; handle 3 stays a proposal, so it is
    /// a model of its own; latch 4 lives in the other final category; product 5 is not on the site.
    /// </summary>
    private static async Task<CatalogDbContext> CatalogAsync()
    {
        var db = CreateContext();
        AddCategory(db, Parent);
        foreach (var child in new[] { Final, OtherFinal })
        {
            var category = ProductCategory.Create(ProductCategoryId.From(child), Root, $"Категорія {child}",
                $"katehoriia-{child}", CategoryPath.From($"n{Parent}.n{child}"), ProductCategoryId.From(Parent)).Value;
            category.SetShownOnSite(true, DateTimeOffset.UtcNow);
            db.Categories.Add(category);
        }
        var products = new (int Id, int Category, string Name, bool InStock, bool OnSite)[]
        {
            (1, Final, "Завіса KEDR AB L", true, true), (2, Final, "Завіса KEDR AB R", false, true),
            (3, Final, "Ручка Linea AB", true, true), (4, OtherFinal, "Засувка AB", true, true),
            (5, Final, "Завіса KEDR AB L 2", true, false)
        };
        foreach (var product in products)
        {
            AddProduct(db, product.Id, product.Category, product.Name);
            db.ProductListProjections.Add(ProductListProjection.Create(ProductId.From(product.Id), product.Name,
                product.Name + " (рос.)", $"product-{product.Id}", $"photo-{product.Id}.jpg",
                ProductCategoryId.From(product.Category), $"katehoriia-{product.Category}", product.InStock,
                isSale: false, isNew: product.Id == 3, exportToSite: product.OnSite, retailPrice: 100m + product.Id));
        }

        await db.SaveChangesAsync();
        await RunAsync(db, Dictionary, CsvHeader + "\n" + string.Join("\n", Row(1, "Завіса", "AB", "L"),
            Row(2, "Завіса", "AB", "R"), Row(3, "Ручка", "AB", "L"), Row(4, "Засувка", "AB", "L")) + "\n", apply: true);

        var hinges = await db.ProductGroupMembers.AsNoTracking()
            .Where(member => member.ProductId == ProductId.From(1)).Select(member => member.GroupId!.Value).SingleAsync();
        var state = new SiteModelStateReader(db);
        var accepted = await new AcceptSiteModelsHandler(db, new TestRepository<ProductGroup>(db), new Manager(),
                new SiteModelChanges(db, new TestRepository<ProductGroupAxis>(db),
                    new global::Catalog.Application.Features.Groups.Shared.ProductGroupPins(db,
                        new TestRepository<ProductGroupMember>(db)), GroupBuilder(db), state),
                state, TimeProvider.System)
            .Handle(new AcceptSiteModelsCommand(new AcceptSiteModelsRequest { Ids = [hinges.Value] }),
                CancellationToken.None);
        Assert.True(accepted.IsSuccess);
        await new SetSiteModelTextHandler(db, new TestRepository<ProductGroupText>(db), new SiteModelReader(state),
                TimeProvider.System)
            .Handle(new SetSiteModelTextCommand(hinges.Value, "uk", new SetSiteModelTextRequest
            {
                ShortDescription = "Завіса для дверей", SeoTitle = "Завіса KEDR"
            }), CancellationToken.None);
        db.ChangeTracker.Clear();
        return db;
    }

    private static PublicModelCatalogReader Reader(CatalogDbContext db)
        => new(db, new FakeProductMediaLibrary(), Options.Create(new RootCategoryId
        {
            DoorsRootCategoryId = "000007226", HardwareRootCategoryId = Root, CosmosRootCategoryId = "Космос",
            CosmosCategoryId = 920000
        }));

    private static async Task<Ardalis.Result.Result<PublicSiteModelListDto>> ListAsync(CatalogDbContext db,
        string? category, GetPublicSiteModelsRequest request, string lang = "uk")
        => await new GetPublicSiteModelsHandler(Reader(db), new SlugService(db))
            .Handle(new GetPublicSiteModelsQuery(lang, category, request), CancellationToken.None);

    private static async Task<(string Attribute, string Left, string Right)> SideSlugsAsync(CatalogDbContext db)
    {
        var side = await db.Attributes.AsNoTracking().SingleAsync(item => item.Code == AttributeImportCatalog.Side);
        var values = await db.AttributeValues.AsNoTracking().Where(value => value.AttributeId == side.Id)
            .ToDictionaryAsync(value => value.Code, value => value.Slug);
        return (side.Slug, values["L"], values["R"]);
    }

    [Fact]
    public async Task FinalCategory_ShowsModelsAndProductsOfTheirOwn_WithFilterCounts_SC_M07()
    {
        await using var db = await CatalogAsync();
        var side = await SideSlugsAsync(db);

        var all = (await ListAsync(db, "katehoriia-11", new GetPublicSiteModelsRequest())).Value;
        Assert.True(all.IsFinalCategory);
        Assert.Equal(["Завіса", "Ручка Linea AB"], all.Items.Select(card => card.Name));
        var hinges = all.Items[0];
        Assert.Equal((true, 2, 101m, "photo-1.jpg", (int?)null), (hinges.IsAccepted, hinges.ProductCount, hinges.PriceFrom, hinges.Photo, hinges.ProductId));
        Assert.Equal(("product-3", false, true, (int?)3), (all.Items[1].Slug, all.Items[1].IsAccepted, all.Items[1].IsNew, all.Items[1].ProductId));
        var sideFilter = Assert.Single(all.Filters, filter => filter.Slug == side.Attribute);
        Assert.Equal([(side.Left, 2), (side.Right, 1)], sideFilter.Values.Select(value => (value.Slug, value.Count)));

        var right = (await ListAsync(db, "katehoriia-11", new GetPublicSiteModelsRequest
        {
            Filter = [$"{side.Attribute}:{side.Right}"]
        })).Value;
        var card = Assert.Single(right.Items);
        Assert.Equal(("Завіса", 102m, false), (card.Name, card.PriceFrom, card.InStock));
        // Its own choice does not narrow the side counts.
        var counted = Assert.Single(right.Filters, filter => filter.Slug == side.Attribute);
        Assert.Equal([(side.Left, 2, false), (side.Right, 1, true)],
            counted.Values.Select(value => (value.Slug, value.Count, value.Selected)));

        var inStockRight = (await ListAsync(db, "katehoriia-11", new GetPublicSiteModelsRequest
        {
            Filter = [$"{side.Attribute}:{side.Right}"], InStock = true
        })).Value;
        Assert.Empty(inStockRight.Items);

        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, (await ListAsync(db, "katehoriia-11",
            new GetPublicSiteModelsRequest { Filter = [$"{side.Attribute}:nemaie"] })).Status);
    }

    [Fact]
    public async Task ParentCategory_ShowsAllChildModels_WithoutAttributeFilters_SC_M08()
    {
        await using var db = await CatalogAsync();
        var side = await SideSlugsAsync(db);

        var parent = (await ListAsync(db, "katehoriia-10", new GetPublicSiteModelsRequest { Sort = "price_desc" })).Value;
        Assert.False(parent.IsFinalCategory);
        Assert.Empty(parent.Filters);
        Assert.Equal(["Засувка AB", "Ручка Linea AB", "Завіса"], parent.Items.Select(card => card.Name));
        Assert.Equal(["katehoriia-12", "katehoriia-11", "katehoriia-11"], parent.Items.Select(card => card.CategorySlug));

        var filtered = await ListAsync(db, "katehoriia-10",
            new GetPublicSiteModelsRequest { Filter = [$"{side.Attribute}:{side.Left}"] });
        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, filtered.Status);
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound,
            (await ListAsync(db, "nemaie", new GetPublicSiteModelsRequest())).Status);
    }

    [Fact]
    public async Task Search_FindsModelsByNameProductNameAndCode_AndAnExactCodeGivesTheAddress_SC_M09()
    {
        await using var db = await CatalogAsync();
        var side = await SideSlugsAsync(db);

        async Task<PublicSiteModelListDto> SearchAsync(string search, string lang = "uk")
            => (await ListAsync(db, null, new GetPublicSiteModelsRequest { Search = search }, lang)).Value;

        Assert.Equal(["Завіса"], (await SearchAsync("завіса")).Items.Select(card => card.Name));
        Assert.Equal(["Завіса"], (await SearchAsync("kedr r")).Items.Select(card => card.Name));
        Assert.Equal(["Ручка Linea AB (рос.)"], (await SearchAsync("linea", "ru")).Items.Select(card => card.Name));
        Assert.Empty((await SearchAsync("2 l")).Items);

        var byCode = await SearchAsync("2");
        Assert.Equal(["Завіса"], byCode.Items.Select(card => card.Name));
        var address = Assert.IsType<PublicSiteModelAddressDto>(byCode.ExactMatch);
        Assert.Equal("katehoriia-11", address.CategorySlug);
        Assert.Equal(new Dictionary<string, string> { [side.Attribute] = side.Right }, address.Selection);
        Assert.Null((await SearchAsync("5")).ExactMatch);
    }

    [Fact]
    public async Task ModelPage_HasAxesProductsTextsAndCharacteristics_AndOldAddressesRedirect_SC_M10()
    {
        await using var db = await CatalogAsync();
        var side = await SideSlugsAsync(db);
        var handler = new GetPublicSiteModelHandler(Reader(db), new SlugService(db));
        var slug = (await ListAsync(db, "katehoriia-11", new GetPublicSiteModelsRequest())).Value.Items[0].Slug;

        var page = (await handler.Handle(new GetPublicSiteModelQuery("ru", slug), CancellationToken.None)).Value.Model!;
        Assert.Equal(("Завіса", true, "katehoriia-11"), (page.Name, page.IsAccepted, page.Category.Slug));
        Assert.Equal(["katehoriia-10", "katehoriia-11"], page.Breadcrumbs.Select(item => item.Slug));
        Assert.Equal(("Завіса для дверей", "Завіса KEDR"), (page.Texts.ShortDescription, page.Texts.SeoTitle));
        var axis = Assert.Single(page.Axes);
        Assert.Equal((side.Attribute, "button"), (axis.Slug, axis.Display));
        Assert.Equal([side.Left, side.Right], axis.Values.Select(value => value.Key));
        Assert.Equal([1, 2], page.Products.Select(product => product.Id));
        Assert.Equal("Завіса KEDR AB R (рос.)", page.Products[1].Name);
        Assert.Equal(side.Right, page.Products[1].Selection[side.Attribute]);
        Assert.Contains(page.Characteristics, item => item.Code == AttributeImportCatalog.Color
                                                      && item.Values.SequenceEqual(["Антична бронза"]));

        var own = (await handler.Handle(new GetPublicSiteModelQuery("uk", "product-3"), CancellationToken.None)).Value;
        Assert.Equal(("Ручка Linea AB", false), (own.Model!.Name, own.Model.IsAccepted));
        Assert.Empty(own.Model.Axes);

        // A product of an accepted model points to its model with its side chosen (M-11).
        var moved = (await handler.Handle(new GetPublicSiteModelQuery("uk", "product-2"), CancellationToken.None)).Value;
        Assert.Null(moved.Model);
        Assert.Equal((slug, side.Right), (moved.Redirect!.ModelSlug, moved.Redirect.Selection[side.Attribute]));
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound,
            (await handler.Handle(new GetPublicSiteModelQuery("uk", "nemaie"), CancellationToken.None)).Status);
    }

    [Fact]
    public async Task ProductAddress_GivesTheModelOfAnOnSiteProduct_AndTheCategoryOfAProductOffTheSite_SC_M11()
    {
        await using var db = await CatalogAsync();
        var side = await SideSlugsAsync(db);
        var handler = new GetPublicProductModelAddressHandler(Reader(db), new SlugService(db), db);

        var hinge = (await handler.Handle(new GetPublicProductModelAddressQuery("product-1"), CancellationToken.None)).Value;
        Assert.Equal(side.Left, hinge.Item!.Selection[side.Attribute]);
        var handle = (await handler.Handle(new GetPublicProductModelAddressQuery("product-3"), CancellationToken.None)).Value;
        Assert.Equal(("product-3", "katehoriia-11"), (handle.Item!.ModelSlug, handle.Item.CategorySlug));
        Assert.Empty(handle.Item.Selection);

        var offSite = (await handler.Handle(new GetPublicProductModelAddressQuery("product-5"), CancellationToken.None)).Value;
        Assert.Null(offSite.Item);
        Assert.Equal("katehoriia-11", offSite.Redirect!.RedirectSlug);
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound,
            (await handler.Handle(new GetPublicProductModelAddressQuery("nemaie"), CancellationToken.None)).Status);
    }

    private sealed class Manager : BuildingBlocks.Application.Users.ICurrentUserService
    {
        public string UserId => "2";
        public bool IsAuthenticated => true;
        public string UserName => "manager";
        public string Email => "manager@example.com";
        public IEnumerable<string> Roles => ["Manager"];
        public IEnumerable<System.Security.Claims.Claim> Claims => [];
        public bool IsInRole(string role) => role == "Manager";
        public bool HasPermission(string permission) => false;
        public string GetClaimValue(string claimType) => string.Empty;
    }
}
