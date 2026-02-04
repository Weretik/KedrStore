using Catalog.Application.Integrations.OneC.Jobs;
using Host.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

Console.WriteLine($"ENV = {builder.Environment.EnvironmentName}");

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddJobsHostServices(builder.Configuration);

var app = builder.Build();

using var scope = app.Services.CreateScope();
var scopeServiceProvider = scope.ServiceProvider;


var job = GetArg(args, "--job");
var rootId = GetArg(args, "--rootId");

if (string.IsNullOrWhiteSpace(job))
{
    Console.WriteLine("Usage: dotnet run -- --job=[full|category|prices|pricetypes|productdetails|stocks] [--rootId=ID]");
    return 1;
}


var cancellationToken = CancellationToken.None;
var jobKey = job.Trim().ToLowerInvariant();

var needsRootId = jobKey is "category" or "prices" or "productdetails" or "stocks";
if (needsRootId && string.IsNullOrWhiteSpace(rootId))
{
    Console.WriteLine($"ERROR: --rootId is required for job '{jobKey}'");
    return 2;
}

try
{
    Console.WriteLine($"[INFO] Executing job: {jobKey}...");

    switch (jobKey)
    {
        case "category":
            await scopeServiceProvider.GetRequiredService<SyncOneCCategoryJob>().RunAsync(rootId!, cancellationToken);
            break;

        case "prices":
            await scopeServiceProvider.GetRequiredService<SyncOneCPricesJob>().RunAsync(rootId!, cancellationToken);
            break;

        case "pricetypes":
            await scopeServiceProvider.GetRequiredService<SyncOneCPriceTypesJob>().RunAsync(cancellationToken);
            break;

        case "productdetails":
            await scopeServiceProvider.GetRequiredService<SyncOneCProductDetailsJob>().RunAsync(rootId!, cancellationToken);
            break;

        case "stocks":
            await scopeServiceProvider.GetRequiredService<SyncOneCStocksJob>().RunAsync(rootId!, cancellationToken);
            break;

        case "full":
            await scopeServiceProvider.GetRequiredService<SyncOneCFullJob>().RunAsync(cancellationToken);
            break;

        default:
            Console.WriteLine($"Unknown job: {job}");
            return 1;
    }

    Console.WriteLine("[SUCCESS] Job finished OK");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[ERROR] Job failed: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}


static string? GetArg(string[] args, string name)
{
    var prefix = (name + "=").ToLowerInvariant();
    var hit = args.FirstOrDefault(a => a.Trim().StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));
    return (hit?.Trim())?[prefix.Length..];
}
