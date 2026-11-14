using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Requisites;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IntegrationTests.Content.Company;

/// <summary>Company requisites over HTTP on an in-memory store: public read, Admin-only edit, change log.</summary>
public sealed class CompanyRequisitesApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private const string Iban = "UA303052990000026007025905216";

    private static object Body(string legalName = "ФОП Овчинников Максим Володимирович", string iban = Iban) => new
    {
        legalName,
        code = "2689400771",
        registrationExtractNumber = "24800170000032534",
        iban,
        bankName = "ПриватБанк",
        bankCode = "305299",
    };

    [Fact]
    public async Task Public_ReadsRequisitesWithoutSignIn()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/requisites"));

        Assert.Equal(Iban, json.RootElement.GetProperty("iban").GetString());
        Assert.Equal("305299", json.RootElement.GetProperty("bankCode").GetString());
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminUpdate_IsForAdminsOnly(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.PutAsJsonAsync("/api/admin/content/requisites", Body("ФОП Новий"));

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Admin_ChangesTheName_AndTheHistoryShowsWhoAndWhat()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var update = await admin.PutAsJsonAsync("/api/admin/content/requisites", Body("ФОП Новий"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        using var same = await admin.PutAsJsonAsync("/api/admin/content/requisites", Body("ФОП Новий"));
        Assert.Equal(HttpStatusCode.OK, same.StatusCode);

        using var history = JsonDocument.Parse(await admin.GetStringAsync("/api/admin/content/requisites/history"));
        var entry = Assert.Single(history.RootElement.EnumerateArray());
        Assert.Equal("test-user", entry.GetProperty("changedBy").GetString());
        var change = Assert.Single(entry.GetProperty("changes").EnumerateArray());
        Assert.Equal("legalName", change.GetProperty("field").GetString());
        Assert.Equal("ФОП Овчинников Максим Володимирович", change.GetProperty("oldValue").GetString());
        Assert.Equal("ФОП Новий", change.GetProperty("newValue").GetString());
    }

    [Fact]
    public async Task Manager_CannotReadTheHistory()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.GetAsync("/api/admin/content/requisites/history");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BadIban_IsAFieldError_AndNothingIsLogged()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var response = await admin.PutAsJsonAsync("/api/admin/content/requisites",
            Body(iban: "UA313052990000026007025905216"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("iban", await response.Content.ReadAsStringAsync());
        Assert.Equal(Iban, (await db.CompanyRequisites.AsNoTracking().SingleAsync()).Iban);
        Assert.Empty(await db.CompanyRequisitesChanges.AsNoTracking().ToListAsync());
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.CompanyRequisites.Add(CompanyRequisites.Create(new CompanyRequisitesValues(
            "ФОП Овчинников Максим Володимирович", "2689400771", "24800170000032534", Iban, "ПриватБанк",
            "305299"), Now).Value);
        db.SaveChanges();
        return db;
    }

    private static WebApplicationFactory<Program> CreateFactory(ContentDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadContentDbContext>();
                services.RemoveAll<ContentDbContext>();
                services.AddSingleton(db);
                services.AddSingleton<IReadContentDbContext>(db);
            });
        });
}
