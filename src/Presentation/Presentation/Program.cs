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

// Например, проверим основные подключения:
LogEnv("ASPNETCORE_ENVIRONMENT");
LogEnv("ConnectionStrings__Default");
LogEnv("POSTGRES_USER");
LogEnv("POSTGRES_DB");
LogEnv("POSTGRES_PASSWORD");
LogEnv("Telegram__BotToken");
LogEnv("Telegram__ChatId");
LogEnv("ADMIN_DEFAULT_PASSWORD");
LogEnv("SEQ_PASSWORD");



// Migrations & Seeders
await app.UseAppMigrations();
await app.UseAppSeeders();

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
