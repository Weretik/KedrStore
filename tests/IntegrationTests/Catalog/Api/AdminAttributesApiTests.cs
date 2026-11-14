using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Catalog.Application.Contracts.Persistence;
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
using Catalog.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Api;

/// <summary>Spec catalog/006 phase 3 over HTTP: problem columns and filters, «Огляд» counts, and access.</summary>
public sealed class AdminAttributesApiTests
{
    private static async Task<CatalogDbContext> SeedAsync()
    {
        var db = CreateContext();
        AddCategory(db, 10);
        foreach (var id in new[] { 1, 2, 3, 4 })
        {
            AddProduct(db, id, 10, $"Ручка {id}");
            var slug = $"tovar-{id}";
            db.ProductListProjections.Add(ProductListProjection.Create(ProductId.From(id), slug, slug, slug, "",
                ProductCategoryId.From(10), "kategoriia-10", inStock: true, isSale: false, isNew: false,
                exportToSite: true, retailPrice: 100m));
        }

        await db.SaveChangesAsync();
        await RunAsync(db, Dictionary, CsvHeader + "\n" + string.Join("\n",
            Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "CR", "L"),
            Row(3, "Завіса", "AB", "R", reasonCode: "unresolved_color"),
            Row(4, "Засувка", "AB", "R", noName: true)) + "\n", apply: true);
        return db;
    }

    [Fact]
    public async Task ProductList_ShowsGroupAndProblems_AndFiltersByThem()
    {
        await using var db = await SeedAsync();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var all = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/products?pageSize=10"));
        var rows = all.RootElement.GetProperty("value").EnumerateArray().ToDictionary(row => row.GetProperty("id").GetInt32());
        Assert.Equal("Ручка", rows[1].GetProperty("groupName").GetString());
        Assert.Equal(0, rows[1].GetProperty("problems").GetArrayLength());
        Assert.Equal("unresolved_color", rows[3].GetProperty("problems")[0].GetProperty("code").GetString());

        using var problems = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/products?problemsOnly=true"));
        Assert.Equal([3, 4], problems.RootElement.GetProperty("value").EnumerateArray().Select(row => row.GetProperty("id").GetInt32()));

        using var noName = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/products?problemReason=no_name"));
        Assert.Equal([4], noName.RootElement.GetProperty("value").EnumerateArray().Select(row => row.GetProperty("id").GetInt32()));

        using var reasons = JsonDocument.Parse(
            await manager.GetStringAsync("/api/admin/catalog-attributes/problem-reasons"));
        Assert.Contains(reasons.RootElement.EnumerateArray(), reason =>
            reason.GetProperty("code").GetString() == "no_name" && reason.GetProperty("productCount").GetInt32() == 1);
    }

    [Fact]
    public async Task Overview_CountsOnSiteProblems_AndNoNameProducts()
    {
        await using var db = await SeedAsync();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var json = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/catalog/overview"));

        Assert.Equal(2, json.RootElement.GetProperty("productsWithProblems").GetInt32());
        Assert.Equal(1, json.RootElement.GetProperty("noNameProducts").GetInt32());
    }

    [Fact]
    public async Task Dictionary_ManagerReads_OnlyAdminEdits()
    {
        await using var db = await SeedAsync();
        var colorId = (await db.Attributes.AsNoTracking().SingleAsync(attribute => attribute.Code == "color")).Id.Value;
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var admin = factory.CreateClientWithRole("Admin");
        var body = new { nameUk = "Колір", nameRu = "Цвет", isFilterable = true, sortOrder = 1 };

        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync($"/api/admin/catalog-attributes/{colorId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await manager.PutAsJsonAsync($"/api/admin/catalog-attributes/{colorId}", body)).StatusCode);

        using var updated = await admin.PutAsJsonAsync($"/api/admin/catalog-attributes/{colorId}", body);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        using var json = JsonDocument.Parse(await updated.Content.ReadAsStringAsync());
        var attribute = json.RootElement.GetProperty("attribute");
        Assert.Equal(("Колір", true), (attribute.GetProperty("nameUk").GetString(),
            attribute.GetProperty("isEditedManually").GetBoolean()));

        using var created = await admin.PostAsJsonAsync($"/api/admin/catalog-attributes/{colorId}/values",
            new { nameUk = "Графіт", sortOrder = 9, swatchHex = "#3a3a3a" });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        using var withValue = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var graphite = withValue.RootElement.GetProperty("values").EnumerateArray()
            .Single(value => value.GetProperty("nameUk").GetString() == "Графіт");
        Assert.Equal(("hrafit", "#3A3A3A"), (graphite.GetProperty("slug").GetString(),
            graphite.GetProperty("swatchHex").GetString()));
    }

    [Fact]
    public async Task NewAttribute_OnlyAdminCreates_WithUniqueSlugAndCode()
    {
        await using var db = await SeedAsync();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var admin = factory.CreateClientWithRole("Admin");
        var body = new { nameUk = "Матеріал ручки", nameRu = "Материал ручки", kind = "enum", isFilterable = true,
            isVariantAxis = false, sortOrder = 40 };

        Assert.Equal(HttpStatusCode.Forbidden,
            (await manager.PostAsJsonAsync("/api/admin/catalog-attributes", body)).StatusCode);

        using var created = await admin.PostAsJsonAsync("/api/admin/catalog-attributes", body);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var attribute = json.RootElement.GetProperty("attribute");
        Assert.Equal(("material-ruchky", "material_ruchky", "enum", true), (attribute.GetProperty("slug").GetString(),
            attribute.GetProperty("code").GetString(), attribute.GetProperty("kind").GetString(),
            attribute.GetProperty("isEditedManually").GetBoolean()));

        using var again = JsonDocument.Parse(await (await admin.PostAsJsonAsync("/api/admin/catalog-attributes",
            body with { })).Content.ReadAsStringAsync());
        Assert.Equal("material-ruchky-2", again.RootElement.GetProperty("attribute").GetProperty("slug").GetString());

        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/admin/catalog-attributes",
            new { nameUk = "Ще колір", kind = "enum", code = "color", sortOrder = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/admin/catalog-attributes",
            new { nameUk = "Розмір", kind = "size", sortOrder = 1 })).StatusCode);

        var id = attribute.GetProperty("id").GetInt32();
        using var value = await admin.PostAsJsonAsync($"/api/admin/catalog-attributes/{id}/values",
            new { nameUk = "Латунь", sortOrder = 1 });
        Assert.Equal(HttpStatusCode.OK, value.StatusCode);
    }

    [Fact]
    public async Task ProductPanel_ManagerSetsValueAndAcceptsReview()
    {
        await using var db = await SeedAsync();
        var colorId = (await db.Attributes.AsNoTracking().SingleAsync(attribute => attribute.Code == "color")).Id.Value;
        var chrome = (await db.AttributeValues.AsNoTracking().SingleAsync(value => value.Code == "CR")).Id.Value;
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var set = await manager.PutAsJsonAsync($"/api/admin/products/3/attributes/{colorId}",
            new { values = new[] { new { valueId = chrome } } });
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);

        using var accepted = await manager.PostAsync("/api/admin/products/3/review/accept", null);
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        using var json = JsonDocument.Parse(await accepted.Content.ReadAsStringAsync());
        Assert.Equal("accepted", json.RootElement.GetProperty("review").GetProperty("state").GetString());

        using var problems = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/products?problemsOnly=true"));
        Assert.Equal([4], problems.RootElement.GetProperty("value").EnumerateArray().Select(row => row.GetProperty("id").GetInt32()));
    }

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadCatalogDbContext>();
                services.RemoveAll<CatalogDbContext>();
                services.AddSingleton(db);
                services.AddSingleton<IReadCatalogDbContext>(db);
            });
        });
}
