using Microsoft.EntityFrameworkCore.Design;

namespace Content.Infrastructure.DataBase;

public sealed class ContentDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>();
        ContentDbContext.Configure(options, "Host=localhost;Database=kedr_design_time;Username=design_time;Password=not-used");
        return new ContentDbContext(options.Options);
    }
}
