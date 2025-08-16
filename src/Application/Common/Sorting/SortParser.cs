namespace Application.Common.Sorting;

public static class SortParser
{
    // "name,-price" -> [("name", false), ("price", true)]
    public static IReadOnlyList<SortTerm> Parse(string? sort)
    {
        var result = new List<SortTerm>();

        if (!string.IsNullOrWhiteSpace(sort))
        {
            foreach (var raw in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var desc  = raw.StartsWith('-');
                var field = desc ? raw[1..] : raw;

                if (ProductSortMap.Keys.ContainsKey(field))
                    result.Add(new SortTerm(Field: field, isDesc: desc));
            }
        }

        if (result.Count == 0)
            result.Add(new SortTerm(ProductSortMap.DefaultField, isDesc: false));

        return result;
    }
}
