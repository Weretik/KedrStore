using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Catalog.Application.Abstractions.Persistence;
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

/// <summary>Spec catalog/007 SC-M06 over HTTP: «Огляд» counts, the models list, accept, edits, and access.</summary>
public sealed class AdminSiteModelsApiTests
{
    private const string Base = "/api/admin/site-models";

    private static async Task<CatalogDbContext> SeedAsync()
    {
        var db = CreateContext();
        AddCategory(db, 10);
        foreach (var id in new[] { 1, 2, 3 })
        {
            AddProduct(db, id, 10, $"Ручка {id}");
            var slug = $"tovar-{id}";
            db.ProductListProjections.Add(ProductListProjection.Create(ProductId.From(id), slug, slug, slug, "",
                ProductCategoryId.From(10), "kategoriia-10", inStock: true, isSale: false, isNew: false,
                exportToSite: true, retailPrice: 100m + id));
        }

        await db.SaveChangesAsync();
        await RunAsync(db, Dictionary, CsvHeader + "\n" + string.Join("\n",
            Row(1, "Ручка", "AB", "L"), Row(2, "Ручка", "AB", "R"), Row(3, "Завіса", "AB", "L")) + "\n", apply: true);
        return db;
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task List_IsForProductManagers(string? role, HttpStatusCode expected)
    {
        await using var db = await SeedAsync();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Base);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Manager_ListsAcceptsAndEditsModels_ButNotTheSlug()
    {
        await using var db = await SeedAsync();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var overview = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/catalog/overview"));
        var siteModels = overview.RootElement.GetProperty("siteModels");
        Assert.Equal(0, siteModels.GetProperty("productsWithoutModel").GetInt32());
        Assert.Equal(2, siteModels.GetProperty("proposedModels").GetInt32());

        using var list = JsonDocument.Parse(await manager.GetStringAsync($"{Base}?status=proposed&search=ручка 2"));
        var handle = Assert.Single(list.RootElement.GetProperty("items").EnumerateArray());
        var id = handle.GetProperty("id").GetInt32();
        Assert.Equal(2, handle.GetProperty("productCount").GetInt32());
        Assert.Equal(101m, handle.GetProperty("minPrice").GetDecimal());
        Assert.Equal("Ручка", handle.GetProperty("name").GetString());
        Assert.Equal(["Сторона"], handle.GetProperty("axes").EnumerateArray().Select(axis => axis.GetString()));

        using var accepted = await manager.PostAsJsonAsync($"{Base}/accept", new { ids = new[] { id } });
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        using var health = JsonDocument.Parse(await accepted.Content.ReadAsStringAsync());
        Assert.Equal(0, health.RootElement.GetProperty("productsWithoutModel").GetInt32());

        using var renamed = await manager.PutAsJsonAsync($"{Base}/{id}", new { name = "Ручка KEDR" });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        using var texts = await manager.PutAsJsonAsync($"{Base}/{id}/texts/uk",
            new { shortDescription = "Ручка для дверей", seoDescription = "Ручка KEDR для міжкімнатних дверей" });
        Assert.Equal(HttpStatusCode.OK, texts.StatusCode);
        using var badLanguage = await manager.PutAsJsonAsync($"{Base}/{id}/texts/en", new { shortDescription = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, badLanguage.StatusCode);

        using var model = JsonDocument.Parse(await manager.GetStringAsync($"{Base}/{id}"));
        Assert.Equal("Ручка KEDR", model.RootElement.GetProperty("name").GetString());
        Assert.Equal("accepted", model.RootElement.GetProperty("status").GetString());
        Assert.True(model.RootElement.GetProperty("axes")[0].GetProperty("isSaved").GetBoolean());
        Assert.Contains(model.RootElement.GetProperty("texts").EnumerateArray(), text =>
            text.GetProperty("language").GetString() == "uk" &&
            text.GetProperty("shortDescription").GetString() == "Ручка для дверей");

        Assert.Equal(HttpStatusCode.Forbidden,
            (await manager.PutAsJsonAsync($"/api/admin/product-groups/{id}/slug", new { slug = "ruchka-kedr" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await manager.GetAsync($"{Base}/999")).StatusCode);
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
