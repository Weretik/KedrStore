namespace Application.Common.Sorting;

public static class SortParser
{
    public static IReadOnlyList<(string key, bool desc)> Parse(string? sort, string defaultKey)
    {
        var list = new List<(string key, bool desc)>();

        if (!string.IsNullOrWhiteSpace(sort))
        {
            foreach (var raw in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var desc = raw.StartsWith('-');
                var key  = (desc ? raw[1..] : raw).ToLowerInvariant();
                list.Add((key, desc));
            }
        }

        if (list.Count == 0) list.Add((defaultKey, false));
        return list;
    }
}
