using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Queries.GetRetailPriceStatus;

/// <summary>The site's retail price type and its prices locked from 1C (spec catalog/010 RP-07).</summary>
public sealed record GetRetailPriceStatusQuery : IQuery<Result<RetailPriceStatusDto>>;
