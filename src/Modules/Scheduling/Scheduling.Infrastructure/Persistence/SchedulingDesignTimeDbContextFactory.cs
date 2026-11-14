using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scheduling.Infrastructure.Persistence;

public sealed class SchedulingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
    public SchedulingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SchedulingDbContext>();
        SchedulingDbContext.Configure(options, "Host=localhost;Database=kedr_design_time;Username=design_time;Password=not-used");
        return new SchedulingDbContext(options.Options);
    }
}
