namespace Domain.Common.ValueObject;

[ValueObject<string>(conversions: Conversions.None)]
public readonly partial struct CategoryPath
{
    public static readonly CategoryPath None = From("n0");

    private static readonly Regex _ltreeRegex = MyRegex();

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Path is required.");

        if (!_ltreeRegex.IsMatch(value))
            return Validation.Invalid("Invalid ltree format.");

        return Validation.Ok;
    }

    public static CategoryPath Root(string node) => From($"n{node}");
    public CategoryPath Append(string node) => From($"{Value}.n{node}");

    [GeneratedRegex(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
