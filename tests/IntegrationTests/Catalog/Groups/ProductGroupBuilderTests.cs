using Microsoft.EntityFrameworkCore;
using global::Catalog.Application.Features.Groups.Common;
using global::Catalog.Application.Features.Groups.Create;
using global::Catalog.Application.Features.Groups.SetForProduct;
using global::Catalog.Application.Features.Groups.SetMain;
using global::Catalog.Application.Slugs;
using global::Catalog.Contracts.Groups;
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
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Groups;

/// <summary>
/// Spec catalog/006 phase 2: automatic variant groups, main product, «Однакові варіанти», manual moves. Since spec
/// catalog/007 R-02 an on-site product left without a group gets a proposed one-product group.
/// </summary>
public sealed class ProductGroupBuilderTests
{
    private static string Csv(params string[] rows) => CsvHeader + "\n" + string.Join("\n", rows) + "\n";

    private static async Task<CatalogDbContext> CatalogAsync(params (int Id, int Category)[] products)
    {
        var db = CreateContext();
        foreach (var category in products.Select(product => product.Category).Distinct())
            AddCategory(db, category);
        foreach (var (id, category) in products)
            AddProduct(db, id, category, $"Товар {id}");
        await db.SaveChangesAsync();
        return db;
    }

    private static async Task<List<ProductGroupMember>> MembersAsync(CatalogDbContext db)
        => await db.ProductGroupMembers.AsNoTracking().OrderBy(member => member.ProductId).ToListAsync();

    [Fact]
    public async Task SameBaseNameInOneCategory_FormsOneGroup_AndSecondRunChangesNothing()
    {
        await using var db = await CatalogAsync((1, 10), (2, 10), (3, 10), (4, 10));
        // Rows are all in category 10; base names differ only by spaces and case for 1 and 2.
        var report = await RunAsync(db, Dictionary, Csv(
            Row(1, "Завіса  KEDR", "AB", "L"), Row(2, "завіса kedr", "CR", "R"), Row(3, "Завіса KEDR", "AB", "R"),
            Row(4, "Ручка", "AB", "L")), apply: true);

        var group = await db.ProductGroups.AsNoTracking().SingleAsync(item => item.Name == "Завіса KEDR");
        Assert.Equal("zavisa-kedr", group.Slug);
        Assert.Equal(1, group.MainProductId?.Value);
        Assert.Equal(ProductGroupStatus.Proposed, group.Status);
        var members = await MembersAsync(db);
        Assert.Equal([1, 2, 3], members.Where(member => member.GroupId == group.Id).Select(member => member.ProductId.Value));
        // The on-site «Ручка» is alone: it gets a one-product proposal (spec catalog/007 R-02).
        var alone = await db.ProductGroups.AsNoTracking().SingleAsync(item => item.Name == "Ручка");
        Assert.Equal(4, alone.MainProductId?.Value);
        Assert.Equal(2, report.Groups.Groups.Created);

        var second = await RunAsync(db, Dictionary, Csv(
            Row(1, "Завіса  KEDR", "AB", "L"), Row(2, "завіса kedr", "CR", "R"), Row(3, "Завіса KEDR", "AB", "R"),
            Row(4, "Ручка", "AB", "L")), apply: true);
        Assert.Equal(2, second.Groups.Groups.Unchanged);
        Assert.Equal(4, second.Groups.Members.Unchanged);
        Assert.Equal(0, second.Groups.ReviewsChanged);
    }

    [Fact]
    public async Task DryRun_ReportsGroupsButWritesNothing()
    {
        await using var db = await CatalogAsync((1, 10), (2, 10));
        await RunAsync(db, Dictionary, Csv(Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "AB", "R")), apply: true);
        db.ProductGroups.RemoveRange(db.ProductGroups);
        await db.SaveChangesAsync();

        var report = await GroupBuilder(db).RunAsync(apply: false, CancellationToken.None);

        Assert.Equal(1, report.Groups.Created);
        Assert.Empty(await db.ProductGroups.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task NoNameAndDeletedProducts_NeverJoinAutomatically()
    {
        await using var db = await CatalogAsync((1, 10), (2, 10), (3, 10));
        await RunAsync(db, Dictionary, Csv(
            Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "AB", "R", noName: true), Row(3, "Ручка", "CR", "L")),
            apply: true);
        Assert.Equal([1, 3], (await MembersAsync(db)).Select(member => member.ProductId.Value));

        var product = await db.Products.SingleAsync(item => item.Id == ProductId.From(3));
        product.MarkDeleted(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var report = await GroupBuilder(db).RunAsync(apply: true, CancellationToken.None);

        // Product 1 is left alone on the site: the group stays as its one-product proposal.
        Assert.Equal(0, report.Groups.Removed);
        Assert.Single(await db.ProductGroups.AsNoTracking().ToListAsync());
        Assert.Equal([1], (await MembersAsync(db)).Select(member => member.ProductId.Value));

        var first = await db.Products.SingleAsync(item => item.Id == ProductId.From(1));
        first.UpdateFromOneC(first.Name, first.CategoryId, 1, exportToSite: false, false, false, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var offSite = await GroupBuilder(db).RunAsync(apply: true, CancellationToken.None);

        Assert.Equal(1, offSite.Groups.Removed);
        Assert.Empty(await db.ProductGroups.AsNoTracking().ToListAsync());
        Assert.Empty(await MembersAsync(db));
    }

    [Fact]
    public async Task MainProduct_PrefersSiteAndStock_AndPersonsChoiceStaysWhileOnSite()
    {
        await using var db = await CatalogAsync((1, 10), (2, 10), (3, 10));
        var first = await db.Products.SingleAsync(item => item.Id == ProductId.From(1));
        first.UpdateFromOneC(first.Name, first.CategoryId, 1, exportToSite: false, false, false, DateTimeOffset.UtcNow);
        (await db.Products.SingleAsync(item => item.Id == ProductId.From(3))).UpdateStock(5);
        await db.SaveChangesAsync();

        await RunAsync(db, Dictionary, Csv(
            Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "AB", "R"), Row(3, "Ручка", "CR", "L")), apply: true);
        var group = await db.ProductGroups.AsNoTracking().SingleAsync();
        Assert.Equal(3, group.MainProductId?.Value);

        var setMain = new SetProductGroupMainCommandHandler(db, new TestRepository<ProductGroup>(db),
            new ProductGroupReader(db), TimeProvider.System);
        var notOnSite = await setMain.Handle(
            new SetProductGroupMainCommand(group.Id.Value, new SetProductGroupMainRequest { ProductId = 1 }),
            CancellationToken.None);
        Assert.False(notOnSite.IsSuccess);

        var chosen = await setMain.Handle(
            new SetProductGroupMainCommand(group.Id.Value, new SetProductGroupMainRequest { ProductId = 2 }),
            CancellationToken.None);
        Assert.Equal(2, chosen.Value.MainProductId);
        await GroupBuilder(db).RunAsync(apply: true, CancellationToken.None);
        db.ChangeTracker.Clear();
        Assert.True((await db.ProductGroups.SingleAsync()).IsMainManual);

        var second = await db.Products.SingleAsync(item => item.Id == ProductId.From(2));
        second.UpdateFromOneC(second.Name, second.CategoryId, 1, exportToSite: false, false, false, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        await GroupBuilder(db).RunAsync(apply: true, CancellationToken.None);
        db.ChangeTracker.Clear();

        group = await db.ProductGroups.SingleAsync();
        Assert.Equal(3, group.MainProductId?.Value);
        Assert.False(group.IsMainManual);
    }

    [Fact]
    public async Task SameVariants_GetAReason_ThatTheImportKeeps_AndAcceptedStaysAccepted()
    {
        await using var db = await CatalogAsync((1, 10), (2, 10), (3, 10));
        string Csv3(string side2) => Csv(
            Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "AB", side2), Row(3, "Ручка", "CR", "L"));

        var report = await RunAsync(db, Dictionary, Csv3("L"), apply: true);
        Assert.Equal(2, report.Groups.ProductsWithSameVariants);
        var review = await db.ProductAttributeReviews.SingleAsync(item => item.ProductId == ProductId.From(1));
        var reason = Assert.Single(review.Reasons);
        Assert.Equal(AttributeReviewReason.SameVariants, reason.Code);
        Assert.Contains("#2", reason.Text, StringComparison.Ordinal);
        Assert.Equal(AttributeReviewState.NeedsReview, review.State);

        review.Accept("manager@example.com", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var again = await RunAsync(db, Dictionary, Csv3("L"), apply: true);
        Assert.Equal(0, again.Reviews.Updated);
        Assert.Equal(0, again.Groups.ReviewsChanged);

        await RunAsync(db, Dictionary, Csv3("R"), apply: true);
        var fixedReview = await db.ProductAttributeReviews.AsNoTracking()
            .SingleAsync(item => item.ProductId == ProductId.From(2));
        Assert.Empty(fixedReview.Reasons);
        Assert.Equal(AttributeReviewState.Clear, fixedReview.State);
        var accepted = await db.ProductAttributeReviews.AsNoTracking()
            .SingleAsync(item => item.ProductId == ProductId.From(1));
        Assert.Equal(AttributeReviewState.Accepted, accepted.State);
    }

    [Fact]
    public async Task PersonsMoves_StayPinned_AndBackToAutomaticRegroups()
    {
        await using var db = await CatalogAsync((1, 10), (2, 10), (3, 10), (4, 10), (5, 20));
        var csv = Csv(Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "AB", "R"), Row(3, "Ручка", "CR", "L"),
            Row(4, "Засувка", "AB", "L"), Row(5, "Засувка", "AB", "L"));
        await RunAsync(db, Dictionary, csv, apply: true);
        var handle = (await db.ProductGroups.AsNoTracking().SingleAsync(group => group.Name == "Ручка")).Id;

        var create = new CreateProductGroupCommandHandler(db, new TestRepository<ProductGroup>(db), new SlugService(db),
            Pins(db), GroupBuilder(db), new ProductGroupReader(db), TimeProvider.System);
        var created = await create.Handle(new CreateProductGroupCommand(new CreateProductGroupRequest
        {
            CategoryId = 10, Name = "Засувки KEDR", ProductIds = [3, 4]
        }), CancellationToken.None);
        Assert.True(created.IsSuccess);
        Assert.True(created.Value.IsCreatedManually);
        Assert.Equal([3, 4], created.Value.Members.Select(member => member.ProductId));

        var wrongCategory = await create.Handle(new CreateProductGroupCommand(new CreateProductGroupRequest
        {
            CategoryId = 10, Name = "Інша", ProductIds = [5]
        }), CancellationToken.None);
        Assert.False(wrongCategory.IsSuccess);

        var move = new SetProductGroupCommandHandler(db, new TestRepository<ProductGroupMember>(db), Pins(db),
            GroupBuilder(db), new ProductGroupReader(db), TimeProvider.System);
        var outOfGroups = await move.Handle(new SetProductGroupCommand(2, new SetProductGroupRequest()),
            CancellationToken.None);
        Assert.True(outOfGroups.Value.IsPinned);
        Assert.Null(outOfGroups.Value.Group);

        // Only product 1 is left automatic in «Ручка»: the group stays as its one-product proposal; the reimport keeps
        // the pins.
        await RunAsync(db, Dictionary, csv, apply: true);
        var members = await MembersAsync(db);
        Assert.Contains(members, member => member.ProductId == ProductId.From(1) && member.GroupId == handle);
        Assert.Contains(members, member => member.ProductId == ProductId.From(2) && member.IsPinned && member.GroupId == null);
        Assert.Equal(2, members.Count(member => member.GroupId == ProductGroupId.Create(created.Value.Id)));

        var back = await move.Handle(new SetProductGroupCommand(2, new SetProductGroupRequest { Automatic = true }),
            CancellationToken.None);
        // The command rebuilds at once: products 1 and 2 form the «Ручка» group again.
        Assert.False(back.Value.IsPinned);
        Assert.Equal([1, 2], back.Value.Group!.Members.Select(member => member.ProductId));
    }

    private static ProductGroupPins Pins(CatalogDbContext db) => new(db, new TestRepository<ProductGroupMember>(db));
}
