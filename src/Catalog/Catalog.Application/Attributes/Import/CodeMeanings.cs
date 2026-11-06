namespace Catalog.Application.Attributes.Import;

/// <summary>
/// What the owner said one code means besides its own attribute (spec catalog/009): values of other attributes for all
/// products, and per category. A category with its own decoding of the code (spec catalog/008) uses only its own
/// meanings, even none.
/// </summary>
public sealed record CodeMeaningSet(
    IReadOnlyList<(string Attribute, string ValueCode)> ForAll,
    IReadOnlyDictionary<int, IReadOnlyList<(string Attribute, string ValueCode)>> ByCategory)
{
    public IReadOnlyList<(string Attribute, string ValueCode)> For(int? categoryId)
        => categoryId is { } id && ByCategory.TryGetValue(id, out var own) ? own : ForAll;
}

/// <summary>
/// Adds the owner's extra meanings of the codes found in a product name to its recognized values. The owner's values win
/// over what the recognizer found for the same attribute and are confirmed.
/// </summary>
public static class CodeMeanings
{
    /// <param name="found">Codes found in the name: (attribute, scope, folded text).</param>
    /// <param name="meanings">Extra meanings by code: (attribute, scope, folded text).</param>
    public static RecognizedProduct Apply(RecognizedProduct product, int? categoryId,
        IEnumerable<(string Attribute, string Scope, string Code)> found,
        IReadOnlyDictionary<(string Attribute, string Scope, string Code), CodeMeaningSet> meanings)
    {
        var added = found
            .OrderBy(code => code.Attribute, StringComparer.Ordinal)
            .ThenBy(code => code.Scope, StringComparer.Ordinal)
            .ThenBy(code => code.Code, StringComparer.Ordinal)
            .SelectMany(code => meanings.TryGetValue(code, out var set) ? set.For(categoryId) : [])
            .Distinct()
            .ToList();
        if (added.Count == 0)
            return product;

        var attributes = added.Select(item => item.Attribute).ToHashSet(StringComparer.Ordinal);
        var values = product.Values.Where(value => !attributes.Contains(value.AttributeCode)).ToList();
        values.AddRange(added
            .GroupBy(item => item.Attribute, StringComparer.Ordinal)
            .SelectMany(group => group.Select((item, position) =>
                new RecognizedValue(item.Attribute, position, item.ValueCode, null, true))));
        return product with { Values = values };
    }
}
