using BuildingBlocks.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.Extensions;
using Presentation.Shared.States.Category;
using BuildingBlocks.Integrations.OneC;
using BuildingBlocks.Integrations.OneC.DependencyInjection;
using Catalog.Application.DependencyInjection;
using Catalog.Infrastructure.DependencyInjection;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.DependencyInjection;
using Presentation.DependencyInjection;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

//---------------------------------------------------------------------------------------
// Serilog
//---------------------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

//---------------------------------------------------------------------------------------
// Razor-components
//---------------------------------------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

//---------------------------------------------------------------------------------------
// Config AdminUser
//---------------------------------------------------------------------------------------
builder.Services
    .Configure<AdminUserConfig>(
        builder.Configuration.GetSection("Identity:AdminUser"));

//---------------------------------------------------------------------------------------
// Application dependency injection
//---------------------------------------------------------------------------------------
builder.Services
    .AddMediatorPipeline()
    .AddValidation();

//---------------------------------------------------------------------------------------
// Infrastructure dependency injection
//---------------------------------------------------------------------------------------
builder.Services
    .AddOneCIntegration()
    .AddInfrastructureServices(builder.Configuration)
    .AddCatalogInfrastructureServices(builder.Configuration)
    .AddIdentityInfrastructure(builder.Configuration);

//---------------------------------------------------------------------------------------
// Fluxor + State services
//---------------------------------------------------------------------------------------
builder.Services.AddFluxor(opt => opt.ScanAssemblies(typeof(SharedAssemblyMarker).Assembly));
builder.Services.AddScoped<IBurgerMenuStore, BurgerMenuStore>();
builder.Services.AddScoped<ICatalogStore, CatalogStore>();
builder.Services.AddScoped<ICategoryStore, CategoryStore>();

//---------------------------------------------------------------------------------------
// Services
//---------------------------------------------------------------------------------------
builder.Services.AddHealthChecks();
builder.Services.AddMudServices();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();


// Логирование всех переменных
void LogEnv(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    Log.Information("{Key} = {Value}", key, string.IsNullOrEmpty(value) ? "<null>" : value);
}

//---------------------------------------------------------------------------------------
// Migrations & Seeders
//---------------------------------------------------------------------------------------
await app.UseAppMigrations();
await app.UseAppSeeders();

//---------------------------------------------------------------------------------------
// SmokeTests OneC
//---------------------------------------------------------------------------------------
if (app.Configuration.GetValue<bool>("OneCSoap:RunSmokeTest"))
{
    await OneCSoapSmokeTest.RunAsync(app.Configuration);
}

//---------------------------------------------------------------------------------------
// Environment
//---------------------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseSerilogRequestLogging();
app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

//---------------------------------------------------------------------------------------
// HealthChecks endpoint
//---------------------------------------------------------------------------------------
app.MapHealthChecks("/health");

app.Run();
