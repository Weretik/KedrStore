using Microsoft.EntityFrameworkCore.Design;

namespace Work.Infrastructure.DataBase;

public sealed class WorkDesignTimeDbContextFactory : IDesignTimeDbContextFactory<WorkDbContext>
{
    public WorkDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WorkDbContext>();
        WorkDbContext.Configure(options, "Host=localhost;Database=kedr_design_time;Username=design_time;Password=not-used");
        return new WorkDbContext(options.Options);
    }
}
