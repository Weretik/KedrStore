using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Contracts.Projections;
using Catalog.Application.Jobs.ProductMedia;
using Catalog.Application.Jobs.Attributes;
using Catalog.Application.Jobs.Slugs;
using Host.Jobs;
using Media.Application.Jobs.CdnSync;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
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

        case "migrate-slugs":
            await scopeServiceProvider.GetRequiredService<MigrateLegacySlugsJob>().RunAsync(cancellationToken);
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

        case "sync-one-c-orders":
            await scopeServiceProvider.GetRequiredService<SyncOneCOrdersJob>().RunAsync(cancellationToken);
            break;

        case "one-c-smoke-write":
            var smokeRequest = CreateOneCSmokeRequest(args);
            var smokeResult = await scopeServiceProvider
                .GetRequiredService<ISalesOneCWriteClient>()
                .SendOrderAsync(smokeRequest, cancellationToken);

            Console.WriteLine(
                $"[RESULT] 1C write outcome={smokeResult.Outcome}; orderNumber={smokeRequest.OrderNumber}; " +
                $"documentId={smokeResult.OneCDocumentId ?? "<none>"}");

            if (smokeResult.Outcome != OneCOrderDeliveryOutcome.Accepted)
                return 3;
            break;

        case "import-catalog-attributes":
            // Without --apply the job only reports what it would change (owner rule: no production data without «да»).
            var dictionaryPath = GetArg(args, "--dictionary") ?? "docs/catalog-attributes/dictionaries.json";
            var csvPath = GetArg(args, "--csv") ?? "docs/catalog-attributes/products_attributes.csv";
            await using (var dictionaryStream = File.OpenRead(dictionaryPath))
            using (var csvReader = new StreamReader(csvPath, System.Text.Encoding.UTF8))
            {
                var importReport = await scopeServiceProvider.GetRequiredService<ImportCatalogAttributesJob>().RunAsync(
                    dictionaryStream, csvReader, args.Contains("--apply", StringComparer.OrdinalIgnoreCase),
                    cancellationToken);
                PrintAttributeReport("import-catalog-attributes", importReport);
            }
            break;

        case "recognize-catalog-attributes":
            // The .NET recognizer over current 1C names (spec catalog/006, phase 4); dry run without --apply.
            var recognizeReport = await scopeServiceProvider.GetRequiredService<RecognizeCatalogAttributesJob>().RunAsync(
                args.Contains("--apply", StringComparer.OrdinalIgnoreCase), productIds: null, cancellationToken);
            PrintAttributeReport("recognize-catalog-attributes", recognizeReport);
            break;

        case "rebuild-projections":
            await scopeServiceProvider.GetRequiredService<IProductListProjectionRebuilder>().RebuildAsync(cancellationToken);
            break;

        case "media-cdn-sync":
            // One run, one schedule: register the CDN files, link them to products (R-33, R-34), then lay them out
            // in folders by category (R-37).
            var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
            var syncReport = await scopeServiceProvider.GetRequiredService<SyncCdnMediaJob>()
                .RunAsync(dryRun, cancellationToken);
            Console.WriteLine(
                $"[RESULT] media-cdn-sync dryRun={syncReport.DryRun}; found={syncReport.FoundOnCdn}; " +
                $"known={syncReport.AlreadyKnown}; registered={syncReport.Registered}; notPictures={syncReport.NotPictures}; " +
                $"orphans={syncReport.Orphans}; missingOnCdn={syncReport.MissingOnCdn}; hiddenSkipped={syncReport.HiddenSkipped}");
            foreach (var (folder, count) in syncReport.RegisteredByFolder.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                Console.WriteLine($"[RESULT] folder {folder}: {count} new");
            var linkReport = await scopeServiceProvider.GetRequiredService<LinkProductMediaJob>()
                .RunAsync(dryRun, cancellationToken);
            Console.WriteLine(
                $"[RESULT] product-media-link dryRun={linkReport.DryRun}; photosLinked={linkReport.PhotosLinked}; " +
                $"schemesLinked={linkReport.SchemesLinked}; linksDropped={linkReport.LinksDropped}; " +
                $"productsChanged={linkReport.ProductsChanged}; withoutPhoto={linkReport.ProductsWithoutPhoto}");
            var folderReport = await scopeServiceProvider.GetRequiredService<ArrangeProductMediaFoldersJob>()
                .RunAsync(dryRun, cancellationToken);
            Console.WriteLine(
                $"[RESULT] product-media-folders dryRun={folderReport.DryRun}; foldersCreated={folderReport.FoldersCreated}; " +
                $"filesMoved={folderReport.FilesMoved}; keptOutside={folderReport.FilesKeptOutside}");
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

static void PrintAttributeReport(string jobName, AttributeImportReport report)
{
    Console.WriteLine($"[RESULT] {jobName} applied={report.Applied}; " +
                      $"products={report.ProductsInFile}; needReview={report.ProductsNeedingReview}");
    Console.WriteLine($"[RESULT] attributes {report.Attributes}");
    Console.WriteLine($"[RESULT] values {report.Values}");
    Console.WriteLine($"[RESULT] aliases {report.Aliases}");
    Console.WriteLine($"[RESULT] product values {report.ProductValues}");
    Console.WriteLine($"[RESULT] reviews {report.Reviews}");
    Console.WriteLine($"[RESULT] category filters {report.CategoryAttributes}");
    var groupReport = report.Groups;
    Console.WriteLine($"[RESULT] groups {groupReport.Groups}");
    Console.WriteLine($"[RESULT] group members {groupReport.Members}; grouped products={groupReport.GroupedProducts}");
    Console.WriteLine($"[RESULT] same variants groups={groupReport.GroupsWithSameVariants}; " +
                      $"products={groupReport.ProductsWithSameVariants}; reviews changed={groupReport.ReviewsChanged}");
    Console.WriteLine($"[RESULT] errors={report.Errors.Count}");
    foreach (var error in report.Errors.Take(AttributeImportReport.MaxErrorsShown))
        Console.WriteLine($"[ERROR] {error}");
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

static OneCOrderDeliveryRequest CreateOneCSmokeRequest(string[] args)
{
    var counterpartyId = RequireArg(args, "--counterpartyId");
    var productId = RequireArg(args, "--productId");
    var orderNumber = RequireArg(args, "--orderNumber");
    var comment = RequireArg(args, "--comment");
    var quantity = ParsePositiveInt(RequireArg(args, "--quantity"), "--quantity");
    var amount = ParseNonNegativeDecimal(RequireArg(args, "--amount"), "--amount");

    return new OneCOrderDeliveryRequest(
        counterpartyId,
        orderNumber,
        DateTimeOffset.UtcNow,
        comment,
        [new OneCOrderDeliveryLineDto(productId, quantity, amount)]);
}

static string RequireArg(string[] args, string name)
    => GetArg(args, name) is { Length: > 0 } value
        ? value
        : throw new ArgumentException($"{name} is required for --job=one-c-smoke-write.");

static int ParsePositiveInt(string value, string name)
    => int.TryParse(value, out var parsed) && parsed > 0
        ? parsed
        : throw new ArgumentException($"{name} must be a positive integer.");

static decimal ParseNonNegativeDecimal(string value, string name)
    => decimal.TryParse(value, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed) && parsed >= 0m
        ? parsed
        : throw new ArgumentException($"{name} must be a non-negative invariant decimal.");
