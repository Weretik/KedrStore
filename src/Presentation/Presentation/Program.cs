using Application.Extensions;
using Infrastructure.Identity.Configuration;
using Infrastructure.Shared.Extensions;
using Presentation.Components;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Seeders — вызываем централизованно
await app.UseAppSeeders(); //  или UseCatalogSeeders или UseIdentitySeeders

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

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Presentation.Client._Imports).Assembly);

app.Run();
