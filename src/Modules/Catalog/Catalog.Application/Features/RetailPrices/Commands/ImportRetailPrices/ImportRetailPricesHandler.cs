using Catalog.Application.Abstractions.Files;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.RetailPrices.Shared;
using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;

/// <summary>
/// Reads the price file, sets the retail prices of the products in it through their roots in two queries and one save,
/// then rebuilds the product list so the Admin and the site show the new prices (spec catalog/010 RP-02…RP-04, RP-08).
/// The file sets target values, so the same file again changes nothing and reports the prices as unchanged.
/// </summary>
public sealed class ImportRetailPricesHandler(
    ICatalogRepository<ProductPrice> priceRepo,
    IReadCatalogDbContext db,
    IRetailPriceSheetReader reader,
    RetailPriceStatusReader status,
    IProductListProjectionRebuilder projection,
    TimeProvider clock,
    ILogger<ImportRetailPricesHandler> logger)
    : ICommandHandler<ImportRetailPricesCommand, Result<RetailPriceImportResultDto>>
{
    private const int ReportedCodes = 50;

    public async ValueTask<Result<RetailPriceImportResultDto>> Handle(ImportRetailPricesCommand command,
        CancellationToken cancellationToken)
    {
        var columnName = await status.ReadNameAsync(cancellationToken);
        if (columnName.Length == 0)
            return Result<RetailPriceImportResultDto>.Invalid(new ValidationError("file",
                "Роздрібного типу ціни ще немає в базі: спершу синхронізуйте типи цін з 1С.", null,
                ValidationSeverity.Error));

        RetailPriceSheet sheet;
        await using (var content = command.File.OpenReadStream())
        {
            var read = reader.Read(content, columnName);
            if (!read.IsSuccess)
                return Result<RetailPriceImportResultDto>.Invalid(read.ValidationErrors.ToArray());
            sheet = read.Value;
        }

        var codes = RetailPriceFileRows.Codes(sheet.Rows).Select(ProductId.Create).ToArray();
        var products = await db.Products.AsNoTracking()
            .Where(product => codes.Contains(product.Id))
            .Select(product => new { product.Id, product.ProductTypeIdOneC })
            .ToDictionaryAsync(product => product.Id, product => product.ProductTypeIdOneC, cancellationToken);
        var plan = RetailPriceFileRows.Plan(sheet.Rows, products.Keys.Select(id => id.Value).ToHashSet());

        var applied = await ApplyAsync(plan, products, command.ProtectFromOneC, cancellationToken);
        if (!await RetailPriceSave.TrySaveAsync(priceRepo, cancellationToken))
            return Result<RetailPriceImportResultDto>.Conflict(RetailPriceSave.ChangedByOneC);

        await projection.RebuildAsync(cancellationToken);

        var withoutPrice = plan.WithoutPriceCodes.Concat(applied.Refused).ToList();
        logger.LogInformation(
            "Retail prices imported: changed {Changed}, unchanged {Unchanged}, not found {NotFound}, " +
            "without price {WithoutPrice}, locked from 1C {Locked}",
            applied.Changed, applied.Unchanged, plan.NotFoundCodes.Count, withoutPrice.Count, command.ProtectFromOneC);

        return Result.Success(new RetailPriceImportResultDto(
            applied.Changed, applied.Unchanged,
            plan.NotFoundCodes.Count, plan.NotFoundCodes.Take(ReportedCodes).ToList(),
            withoutPrice.Count, withoutPrice.Take(ReportedCodes).ToList(),
            await status.ReadAsync(cancellationToken)));
    }

    private async Task<Applied> ApplyAsync(RetailPriceFilePlan plan, IReadOnlyDictionary<ProductId, string> products,
        bool lockFromOneC, CancellationToken cancellationToken)
    {
        var retail = status.RetailPriceTypeId;
        var ids = plan.Prices.Keys.ToArray();
        // Tracked on purpose: the prices found here are changed through their root and saved by the caller.
        var existing = await db.ProductPrices
            .Where(price => price.PriceTypeId == retail && ids.Contains(price.ProductId))
            .ToDictionaryAsync(price => price.ProductId, cancellationToken);

        var now = clock.GetUtcNow();
        var result = new Applied();
        foreach (var (productId, amount) in plan.Prices)
        {
            if (existing.TryGetValue(productId, out var price))
            {
                var set = price.SetFromFile(amount, lockFromOneC, now);
                if (set.IsFailure)
                    result.Refused.Add(Code(productId));
                else if (set.Value)
                    result.Changed++;
                else
                    result.Unchanged++;
                continue;
            }

            var created = ProductPrice.CreateFromFile(products[productId], productId, retail, amount, lockFromOneC, now);
            if (created.IsFailure)
            {
                result.Refused.Add(Code(productId));
                continue;
            }

            await priceRepo.AddAsync(created.Value, cancellationToken);
            result.Changed++;
        }

        return result;
    }

    private static string Code(ProductId productId) => productId.Value.ToString(CultureInfo.InvariantCulture);

    private sealed class Applied
    {
        public int Changed { get; set; }
        public int Unchanged { get; set; }
        public List<string> Refused { get; } = [];
    }
}
