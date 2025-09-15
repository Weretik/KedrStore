namespace Application.Common.Sorting;

public static class SortParser
{
    public static IReadOnlyList<SortToken> ParseStrict(string? sort, string defaultKey)
    {
        var tokens = new List<SortToken>();
        var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(sort))
        {
            var sortArray = sort
                .Replace(" desc", ":desc", StringComparison.OrdinalIgnoreCase)
                .Replace(" asc",  ":asc",  StringComparison.OrdinalIgnoreCase)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


            foreach (var raw in sortArray)
            {
                var trimRaw = raw.Trim();
                var direction = SortDirection.Asc;

                if (trimRaw.StartsWith('-'))
                {
                    direction = SortDirection.Desc;
                    trimRaw = trimRaw[1..].Trim();
                }

                var parts = trimRaw.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var key = parts[0];
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (parts.Length >= 2 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    direction = SortDirection.Desc;
                }
                else if (parts.Length < 2)
                {
                    direction = SortDirection.Asc;
                }

                if (seen.Add(key)) tokens.Add(new SortToken(key, direction));
            }
        }

        if (tokens.Count == 0) tokens.Add(new SortToken(defaultKey, SortDirection.Asc));

        return tokens;
    }
}
