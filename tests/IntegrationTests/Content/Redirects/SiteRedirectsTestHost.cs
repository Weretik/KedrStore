using Content.Application.Abstractions.Persistence;
using Content.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Content.Redirects;

/// <summary>The API on an in-memory content store shared by the requests of one test.</summary>
internal static class SiteRedirectsTestHost
{
    public static ContentDbContext CreateDb() => new(new DbContextOptionsBuilder<ContentDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public static WebApplicationFactory<Program> CreateFactory(ContentDbContext db)
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
