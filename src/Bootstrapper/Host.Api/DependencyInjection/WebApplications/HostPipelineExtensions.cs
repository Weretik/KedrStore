using Host.Api.DependencyInjection.WebApplications.HostPipelines;

namespace Host.Api.DependencyInjection.WebApplications;

public static class HostPipelineExtensions
{
    public static WebApplication UseHostPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
        }
        else
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseCors("SpaDev");

        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.AddHangfireRecurringJobs();
        app.UseHangfireDashboard("/hangfire");

        return app;
    }
}
