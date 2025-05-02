using Application.Common;
using Application.Common.Settings;
using Domain.Entities;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Web.Middlewares;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/kedr-log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();


builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(option =>
    {
        option.Password.RequiredLength = 6;
        option.Password.RequireDigit = true;
        option.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpClient();
builder.Services.Configure<ImageSettings>(
    builder.Configuration.GetSection("ImageSettings"));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (httpContext.Request.Path.StartsWithSegments("/css") ||
            httpContext.Request.Path.StartsWithSegments("/js") ||
            httpContext.Request.Path.StartsWithSegments("/images"))
            return LogEventLevel.Debug;

        return LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var user = httpContext.User.Identity?.Name ?? "Anonymous";
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        diagnosticContext.Set("User", user);
        diagnosticContext.Set("ClientIP", ip);
    };
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var settings = services.GetRequiredService<IOptions<ImageSettings>>().Value;
    var httpClient = services.GetRequiredService<HttpClient>();

    var logger = services.GetRequiredService<ILogger<XmlSeeder>>();
    var seeder = new XmlSeeder(httpClient, logger);
    await seeder.SeedAsync(context, settings);
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");



app.Run();
