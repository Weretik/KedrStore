using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Contracts.Projections;
using Host.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Infrastructure.Integrations.OneC.Jobs;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
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
var logger = scopeServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Host.Jobs");

var job = GetArg(args, "--job");
var rootIds = GetArgs(args, "--rootId");

if (string.IsNullOrWhiteSpace(job))
{
    Console.WriteLine("ERROR: --job is required");
    return 1;
}

using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

var cancellationToken = cancellationTokenSource.Token;
var jobKey = job.Trim().ToLowerInvariant();

var needsRootId = jobKey is "category" or "prices" or "productdetails" or "stocks";
if (needsRootId && rootIds.Count == 0)
{
    Console.WriteLine("ERROR: --rootId is required for this job");
    return 2;
}

try
{
    Console.WriteLine($"[INFO] Executing job: {jobKey}...");

    switch (jobKey)
    {
        case "migrate":
            await scopeServiceProvider.GetRequiredService<DatabaseMigrationJob>().RunAsync(cancellationToken);
            break;

        case "full":
            await scopeServiceProvider.GetRequiredService<SyncOneCFullJob>().RunAsync(cancellationToken);
            break;

        case "pricetypes":
            await scopeServiceProvider.GetRequiredService<SyncOneCPriceTypesJob>().RunAsync(cancellationToken);
            break;

        case "category":
            foreach (var rid in rootIds)
                await scopeServiceProvider.GetRequiredService<SyncOneCCategoryJob>().RunAsync(rid, cancellationToken);
            break;

        case "productdetails":
            foreach (var rid in rootIds)
                await scopeServiceProvider.GetRequiredService<SyncOneCProductDetailsJob>().RunAsync(rid, cancellationToken);
            break;

        case "stocks":
            foreach (var rid in rootIds)
                await scopeServiceProvider.GetRequiredService<SyncOneCStocksJob>().RunAsync(rid, cancellationToken);
            break;

        case "prices":
            foreach (var rid in rootIds)
                await scopeServiceProvider.GetRequiredService<SyncOneCPricesJob>().RunAsync(rid, cancellationToken);
            break;

        case "counterparties":
            await scopeServiceProvider.GetRequiredService<SyncOneCCounterpartiesJob>().RunAsync(cancellationToken);
            break;

        case "counterparty-category-price-types":
            await scopeServiceProvider.GetRequiredService<SyncOneCCounterpartyCategoryPriceTypesJob>().RunAsync(cancellationToken);
            break;

        case "sales-customers-full":
            await scopeServiceProvider.GetRequiredService<SyncOneCSalesCustomersFullJob>().RunAsync(cancellationToken);
            break;

        case "rebuild-projections":
            await scopeServiceProvider.GetRequiredService<IProductListProjectionRebuilder>().RebuildAsync(cancellationToken);
            break;

        default:
            Console.WriteLine($"Unknown job: {jobKey}");
            return 1;
    }

    Console.WriteLine("[SUCCESS] Job finished OK");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Job {JobKey} failed.", jobKey);
    return 1;
}

static string? GetArg(string[] args, string name)
{
    var prefix = name + "=";
    var hit = args.FirstOrDefault(a => a.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    return hit is null ? null : hit.Trim().Substring(prefix.Length);
}

static List<string> GetArgs(string[] args, string name)
{
    var prefix = name + "=";
    return args
        .Select(a => a.Trim())
        .Where(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        .Select(a => a.Substring(prefix.Length))
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .ToList();
}
