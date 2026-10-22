using Microsoft.EntityFrameworkCore.Design;

namespace Sales.Infrastructure.DataBase;

public sealed class SalesDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql("Host=localhost;Database=kedr_design_time;Username=design_time;Password=not-used")
            .Options);
}
