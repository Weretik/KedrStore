using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Categories;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Infrastructure.Persistence;
using ClosedXML.Excel;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Catalog.RetailPrices;

/// <summary>
/// Spec catalog/010 over HTTP on an in-memory catalog: the multipart upload, the flag, the status, the unlock, and
/// who may use them (RP-02, RP-04, RP-06, RP-07, RP-09).
/// </summary>
public sealed class RetailPricesApiTests
{
    private const string Route = "/api/admin/retail-prices";

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    public async Task RetailPrices_AreForStaffWhoManageProducts(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        Assert.Equal(expected, (await client.GetAsync(Route)).StatusCode);
        using var import = await client.PostAsync($"{Route}/import", Form(Workbook(("561", 201)), protect: null));
        Assert.Equal(expected, import.StatusCode);
    }

    [Fact]
    public async Task Manager_UploadsTheFile_LockedByDefault_ThenUnlocks()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var import = await manager.PostAsync($"{Route}/import",
            Form(Workbook(("561", 201), ("999", 10)), protect: null));
        Assert.Equal(HttpStatusCode.OK, import.StatusCode);
        using (var result = JsonDocument.Parse(await import.Content.ReadAsStringAsync()))
        {
            Assert.Equal(1, result.RootElement.GetProperty("changed").GetInt32());
            Assert.Equal("999", result.RootElement.GetProperty("notFoundCodes")[0].GetString());
            Assert.Equal(1, result.RootElement.GetProperty("status").GetProperty("lockedCount").GetInt32());
        }

        using (var status = JsonDocument.Parse(await manager.GetStringAsync(Route)))
        {
            Assert.Equal(11, status.RootElement.GetProperty("priceTypeId").GetInt32());
            Assert.Equal("Розница халтмаг 30%", status.RootElement.GetProperty("priceTypeName").GetString());
            Assert.Equal(1, status.RootElement.GetProperty("lockedCount").GetInt32());
        }

        using var unlock = await manager.PostAsync($"{Route}/unlock", null);
        using var unlocked = JsonDocument.Parse(await unlock.Content.ReadAsStringAsync());
        Assert.Equal(0, unlocked.RootElement.GetProperty("lockedCount").GetInt32());
    }

    [Fact]
    public async Task Upload_WithoutProtection_LeavesThePriceToOneC()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var import = await manager.PostAsync($"{Route}/import", Form(Workbook(("561", 201)), protect: false));

        using var result = JsonDocument.Parse(await import.Content.ReadAsStringAsync());
        Assert.Equal(0, result.RootElement.GetProperty("status").GetProperty("lockedCount").GetInt32());
    }

    [Fact]
    public async Task Upload_OfAFileThatIsNotExcel_Answers400WithTheReason()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var import = await manager.PostAsync($"{Route}/import", Form("text"u8.ToArray(), protect: null));

        Assert.Equal(HttpStatusCode.BadRequest, import.StatusCode);
        Assert.Contains("Це не файл Excel", await import.Content.ReadAsStringAsync());
    }

    private static byte[] Workbook(params (string Code, decimal Price)[] rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Sheet1");
        sheet.Cell("A1").Value = "Каталог товаров";
        sheet.Cell("B3").Value = "Код";
        sheet.Cell("C3").Value = "Розница халтмаг 30%";
        for (var i = 0; i < rows.Length; i++)
        {
            sheet.Cell(4 + i, 2).Value = rows[i].Code;
            sheet.Cell(4 + i, 3).Value = rows[i].Price;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static MultipartFormDataContent Form(byte[] bytes, bool? protect)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType =
            new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(file, "file", "Цены_26.11.xlsx");
        if (protect is not null)
            form.Add(new StringContent(protect.Value ? "true" : "false"), "protectFromOneC");
        return form;
    }

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.PriceTypes.Add(PriceType.Create(PriceTypeId.Create(11), "Розница халтмаг 30%"));
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(10), "000005513", "Броненакладки",
            "bronenakladky", CategoryPath.Create("n10")).Value);
        db.Products.Add(Product.Create(ProductId.Create(561), "000005513", "CLASS Броненакладка QVADRO PROTECT AB",
            "qvadro-protect-ab", ProductCategoryId.Create(10), DateTimeOffset.UtcNow, 5, 1, false, false, true).Value);
        db.SaveChanges();
        return db;
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
                // The in-memory store has no SQL; the PostgreSQL tests cover the list rebuild.
                services.RemoveAll<IProductListProjectionRebuilder>();
                services.AddSingleton<IProductListProjectionRebuilder, NoopProjections>();
            });
        });

    private sealed class NoopProjections : IProductListProjectionRebuilder
    {
        public Task RebuildAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int> RefreshPhotosAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
