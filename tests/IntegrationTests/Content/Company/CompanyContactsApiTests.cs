using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Contacts;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IntegrationTests.Content.Company;

/// <summary>Company contacts over HTTP on an in-memory content store: public read, admin access, validation.</summary>
public sealed class CompanyContactsApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    private static readonly object ValidBody = new
    {
        salesPhone = "+380501112233",
        email = "sales@classkedr.com.ua",
        telegramUrl = "https://t.me/classkedr",
        viberPhone = "+380501112233",
        mapUrl = "https://maps.app.goo.gl/new",
        cityUk = "Харків",
        cityRu = "Харьков",
    };

    [Fact]
    public async Task Public_ReadsContactsWithoutSignIn()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/contacts"));

        Assert.Equal("+380687135049", json.RootElement.GetProperty("salesPhone").GetString());
        Assert.Equal("Харьков", json.RootElement.GetProperty("cityRu").GetString());
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task AdminUpdate_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.PutAsJsonAsync("/api/admin/content/contacts", ValidBody);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Manager_UpdatesContacts_AndTheSiteReadsTheNewPhone()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var update = await manager.PutAsJsonAsync("/api/admin/content/contacts", ValidBody);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        using var saved = JsonDocument.Parse(await update.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrEmpty(saved.RootElement.GetProperty("updatedBy").GetString()));

        using var client = factory.CreateClient();
        using var json = JsonDocument.Parse(await client.GetStringAsync("/api/content/contacts"));
        Assert.Equal("+380501112233", json.RootElement.GetProperty("salesPhone").GetString());
        Assert.Equal("https://t.me/classkedr", json.RootElement.GetProperty("telegramUrl").GetString());
        Assert.False(json.RootElement.TryGetProperty("updatedBy", out _), "the site never sees who edited");
    }

    [Fact]
    public async Task InvalidPhone_IsAFieldError_AndNothingChanges()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.PutAsJsonAsync("/api/admin/content/contacts", new
        {
            salesPhone = "068 713 50 49",
            email = "sales@classkedr.com.ua",
            telegramUrl = "https://t.me/classkedr",
            viberPhone = "+380501112233",
            mapUrl = "https://maps.app.goo.gl/new",
            cityUk = "Харків",
            cityRu = "Харьков",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("salesPhone", await response.Content.ReadAsStringAsync());
        Assert.Equal("+380687135049", (await db.CompanyContacts.AsNoTracking().SingleAsync()).SalesPhone);
    }

    private static ContentDbContext Seed()
    {
        var db = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.CompanyContacts.Add(CompanyContacts.Create(new CompanyContactsValues("+380687135049",
            "kedr.class19@gmail.com", "https://t.me/+380687135049", "+380687135049",
            "https://maps.app.goo.gl/D2rH3HcaxLfxik5H6", "Харків", "Харьков"), Now).Value);
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
