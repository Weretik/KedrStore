using Identity.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IntegrationTests.Identity.Persistence;

public sealed class IdentityMigrationLineageTests
{
    [Fact]
    public void GuidInitialMigration_CreatesIdentitySchemaFromEmptyBaseline()
    {
        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseNpgsql("Host=localhost;Database=not_used;Username=not_used;Password=not_used")
            .Options;
        using var dbContext = new AppIdentityDbContext(options);

        var migrations = dbContext.Database.GetMigrations().ToArray();
        var migrator = dbContext.GetService<IMigrator>();
        var script = migrator.GenerateScript(fromMigration: null, toMigration: migrations[0]);

        Assert.Equal("20260529142739_InitialAppIdentityDbContext", migrations[0]);
        Assert.Contains("CREATE TABLE \"AspNetRoles\"", script, StringComparison.Ordinal);
        Assert.Contains("uuid", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE \"AppRefreshSessions\"", script, StringComparison.Ordinal);
    }

    [Fact]
    public void DataProtectionMigration_AddsKeysTableWithoutTouchingSystemXmin()
    {
        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseNpgsql("Host=localhost;Database=not_used;Username=not_used;Password=not_used")
            .Options;
        using var dbContext = new AppIdentityDbContext(options);

        var migrator = dbContext.GetService<IMigrator>();
        var script = migrator.GenerateScript(
            fromMigration: "20260529142739_InitialAppIdentityDbContext",
            toMigration: "20261028140559_PersistDataProtectionKeysAndRefreshConcurrency");

        Assert.Contains("CREATE TABLE \"DataProtectionKeys\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("xmin", script, StringComparison.OrdinalIgnoreCase);
    }
}
