namespace Application.Common.Validation;

public static class ValidationErrorMapper
{
    public static Dictionary<string, string[]> Map(AppError error)
    {
        if (string.IsNullOrWhiteSpace(error.Details))
            return new();

        return error.Details
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split(':', 2))
            .Where(pair => pair.Length == 2)
            .GroupBy(pair => pair[0].Trim(), pair => pair[1].Trim())
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
    public static string Flatten(this Dictionary<string, string[]> errors)
    {
        return string.Join("; ", errors.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value)}"));
    }
}
