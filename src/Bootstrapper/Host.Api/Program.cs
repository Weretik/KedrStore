using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Host.Api.Http.OpenApi;
using Catalog.Api.DependencyInjection;
using Content.Api.DependencyInjection;
using Identity.Api.DependencyInjection;
using Media.Api.DependencyInjection;
using Sales.Api.DependencyInjection;
using Work.Api.DependencyInjection;
using Scheduling.Api.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Fail at startup, in every environment, when a Singleton holds a Scoped service (a captive DbContext) or a
// registration is missing. Development enables this by default; production must too.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.AddObservability();
builder.Services.AddHostServices(builder.Configuration);
builder.Services.WithoutBackgroundWorkForBuildTimeDocument();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapApiDocument();
/*
    try
    {
        using var scope = app.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<SyncOneCCategoryJob>();

        await job.RunAsync("000005513", app.Lifetime.ApplicationStopping); // hardware
        await job.RunAsync("000007226", app.Lifetime.ApplicationStopping); // doors
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "SyncOneCCategoryJob failed during Development startup.");
    }
*/
}
else
{
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseTrustedProxyClientIp();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseMobileSyncCompression();
app.UseRateLimiter();
app.UseCookieCsrfProtection();
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkActivity();

app.MapCatalogEndpoints();
app.MapContentEndpoints();
app.MapIdentityEndpoints();
app.MapMediaEndpoints();
app.MapSalesEndpoints();
app.MapWorkEndpoints();
app.MapSchedulingEndpoints();
// Liveness answers without dependencies; readiness also checks PostgreSQL (operations: diagnostics).
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(DatabaseHealthCheck.ReadyTag),
}).AllowAnonymous();

app.Run();
