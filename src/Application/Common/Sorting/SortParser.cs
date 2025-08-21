namespace Application.Common.Sorting;

public static class SortParser
{
    public static IReadOnlyList<SortToken> ParseStrict(string? sort, string defaultKey)
    {
        var tokens = new List<SortToken>();
        var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(sort))
        {
            var sortArray = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var raw in sortArray)
            {
                var parts = raw.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length is 0) continue;

                var key = parts[0];
                var direction = SortDirection.Asc;
                if (parts.Length >= 2)
                {
                    var d = parts[1];
                    if (d.Equals("desc", StringComparison.OrdinalIgnoreCase)) direction = SortDirection.Desc;
                    else if (d.Equals("asc",  StringComparison.OrdinalIgnoreCase)) direction = SortDirection.Asc;
                }

                if (string.IsNullOrWhiteSpace(key)) continue;
                if (seen.Add(key)) tokens.Add(new SortToken(key, direction));
            }
        }

        if (tokens.Count == 0) tokens.Add(new SortToken(defaultKey, SortDirection.Asc));

        return tokens;
    }
}
