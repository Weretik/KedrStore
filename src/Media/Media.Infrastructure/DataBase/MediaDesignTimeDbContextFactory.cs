using Microsoft.EntityFrameworkCore.Design;

namespace Media.Infrastructure.DataBase;

public sealed class MediaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    public MediaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>();
        MediaDbContext.Configure(options, "Host=localhost;Database=kedr_design_time;Username=design_time;Password=not-used");
        return new MediaDbContext(options.Options);
    }
}
