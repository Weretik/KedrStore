using System.Net;
using System.Text.Json;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static IntegrationTests.Catalog.Attributes.AttributeImportTestSupport;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Api;

/// <summary>Spec catalog/007 stage 5 over HTTP: anonymous model lists, the model page, and the old product address.</summary>
public sealed class PublicSiteModelsApiTests
{
    private static async Task<CatalogDbContext> SeedAsync()
    {
        var db = CreateContext();
        AddCategory(db, 10);
        foreach (var id in new[] { 1, 2 })
        {
            AddProduct(db, id, 10, $"Ручка {id}");
            db.ProductListProjections.Add(ProductListProjection.Create(ProductId.From(id), $"Ручка {id}",
                $"Ручка {id}", $"product-{id}", "", ProductCategoryId.From(10), "katehoriia-10", inStock: true,
                isSale: false, isNew: false, exportToSite: true, retailPrice: 100m + id));
        }

        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Anonymous_ListsModels_OpensAModel_AndResolvesAProductAddress()
    {
        await using var db = await SeedAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var list = JsonDocument.Parse(await client.GetStringAsync("/api/catalog/uk/katehoriia-10/models?sort=name_asc"));
        Assert.True(list.RootElement.GetProperty("isFinalCategory").GetBoolean());
        Assert.Equal(["product-1", "product-2"], list.RootElement.GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("slug").GetString()));

        using var search = JsonDocument.Parse(await client.GetStringAsync("/api/catalog/uk/models?search=2"));
        Assert.Equal("product-2", search.RootElement.GetProperty("exactMatch").GetProperty("modelSlug").GetString());

        using var model = JsonDocument.Parse(await client.GetStringAsync("/api/catalog/ru/model/product-1"));
        Assert.Equal("Ручка 1", model.RootElement.GetProperty("name").GetString());
        Assert.False(model.RootElement.GetProperty("isAccepted").GetBoolean());

        using var address = JsonDocument.Parse(await client.GetStringAsync("/api/catalog/uk/product/product-2/model"));
        Assert.Equal("katehoriia-10", address.RootElement.GetProperty("categorySlug").GetString());

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/catalog/uk/model/nemaie")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/catalog/uk/nemaie/models")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/catalog/uk/models?sort=random")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/catalog/uk/katehoriia-10/models?filter=bez-dvokrapky")).StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReadCatalogDbContext>();
                services.RemoveAll<CatalogDbContext>();
                services.AddSingleton(db);
                services.AddSingleton<IReadCatalogDbContext>(db);
            });
        });
}
