using Application.Common.Extensions;
using Infrastructure.Common.Extensions;
using Presentation.Shared.States.Category;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Razor-компоненты
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Конфигурация AdminUser
builder.Services.Configure<AdminUserConfig>(
    builder.Configuration.GetSection("Identity:AdminUser"));

// DI: Application + Infrastructure
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// DI: Fluxor + State services
builder.Services.AddFluxor(opt => opt.ScanAssemblies(typeof(SharedAssemblyMarker).Assembly));
builder.Services.AddScoped<IBurgerMenuStore, BurgerMenuStore>();
builder.Services.AddScoped<ICatalogStore, CatalogStore>();
builder.Services.AddScoped<ICategoryStore, CategoryStore>();

// Services
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

// Migrations & Seeders
await app.UseAppMigrations();
await app.UseAppSeeders();

// SmokeTests
if (app.Configuration.GetValue<bool>("OneCSoap:RunSmokeTest"))
{
    await Infrastructure.Common.Integrations.OneC.OneCSoapSmokeTest.RunAsync(app.Configuration);
}

//Environment
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

// HealthChecks endpoint
app.MapHealthChecks("/health");

app.Run();
