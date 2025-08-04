var builder = WebApplication.CreateBuilder(args);

// Подгружаем .env ТОЛЬКО если локальная разработка
if (builder.Environment.IsDevelopment())
{
    Env.TraversePath().Load();
}

// Конфигурация: переменные окружения → appsettings.json
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(builder.Configuration)
                        .Enrich.FromLogContext()
                        .CreateLogger();

// Razor-компоненты
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Конфигурация AdminUser
builder.Services.Configure<AdminUserConfig>(
    builder.Configuration.GetSection("Identity:AdminUser"));

// DI: States
builder.Services.Scan(scan => scan
    .FromAssemblyOf<SharedAssemblyMarker>()
    .AddClasses(c => c.AssignableTo<IState>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// DI: State Container
builder.Services.AddScoped<StateContainer>();

// DI: Application + Infrastructure
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// Services
builder.Services.AddHealthChecks();
builder.Host.UseSerilog();
builder.Services.AddMudServices();

var app = builder.Build();

// Migrations
await app.UseAppMigrations();
// Seeders — вызываем централизованно
await app.UseAppSeeders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Presentation.Client._Imports).Assembly);

// HealthChecks endpoint
app.MapHealthChecks("/health");

app.Run();
