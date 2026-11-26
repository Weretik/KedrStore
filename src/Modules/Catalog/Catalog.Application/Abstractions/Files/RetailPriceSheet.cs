namespace Catalog.Application.Abstractions.Files;

/// <summary>The rows of the price list under its header, in file order.</summary>
public sealed record RetailPriceSheet(IReadOnlyList<RetailPriceSheetRow> Rows);

/// <summary>
/// One row: the «Код» cell as text (empty for title and group rows) and the retail price cell as a number, or null
/// when it is empty or not a number.
/// </summary>
public sealed record RetailPriceSheetRow(string Code, decimal? Price);
