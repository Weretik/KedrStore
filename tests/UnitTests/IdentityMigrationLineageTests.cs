using Identity.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UnitTests;

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
        var script = migrator.GenerateScript(fromMigration: null, toMigration: migrations.Single());

        Assert.Equal(["20260529142739_InitialAppIdentityDbContext"], migrations);
        Assert.Contains("CREATE TABLE \"AspNetRoles\"", script, StringComparison.Ordinal);
        Assert.Contains("uuid", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE \"AppRefreshSessions\"", script, StringComparison.Ordinal);
    }
}
