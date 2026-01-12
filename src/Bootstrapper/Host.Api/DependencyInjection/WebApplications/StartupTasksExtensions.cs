namespace Host.Api.DependencyInjection.WebApplications;

public static class StartupTasksExtensions
{
    public static async Task<WebApplication> RunStartupTasksAsync(this WebApplication app)
    {
        await app.UseAppMigrations();
        await app.UseAppSeeders();
        return app;
    }
}
