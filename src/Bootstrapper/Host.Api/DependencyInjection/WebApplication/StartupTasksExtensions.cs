using AspNetWebApplication = Microsoft.AspNetCore.Builder.WebApplication;

namespace Host.Api.DependencyInjection.WebApplication;


public static class StartupTasksExtensions
{
    public static async Task<AspNetWebApplication> RunStartupTasksAsync(this AspNetWebApplication app)
    {
        await app.UseAppMigrations();
        await app.UseAppSeeders();

        return app;
    }
}
