namespace Catalog.Domain.Categories;

[ValueObject<string>(conversions: Conversions.None)]
public readonly partial struct CategoryPath
{
    private static readonly Regex _ltreeRegex = MyRegex();
    public static CategoryPath None => From("n0");

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Потрібне місце категорії в дереві.");

        if (!_ltreeRegex.IsMatch(value))
            return Validation.Invalid("Некоректне місце категорії в дереві.");

        return Validation.Ok;
    }

    public static CategoryPath Root(string node) => From($"n{node}");
    public CategoryPath Append(string node) => From($"{Value}.n{node}");

    [GeneratedRegex(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
