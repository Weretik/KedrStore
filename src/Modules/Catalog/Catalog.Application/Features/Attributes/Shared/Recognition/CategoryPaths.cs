namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>A category of the catalog tree as the recognizer needs it.</summary>
public sealed record RecognitionCategory(int Id, int? ParentId, string Name);

/// <summary>
/// Full category paths («Фурнітура &gt; Ручки») and path problems (a missing parent or a cycle), built like the
/// Python recognizer: Ukrainian short name, else name, else the ID.
/// </summary>
public sealed class CategoryPaths
{
    public IReadOnlyDictionary<int, string> Paths { get; }
    public IReadOnlyDictionary<int, IReadOnlyList<string>> Issues { get; }

    private CategoryPaths(IReadOnlyDictionary<int, string> paths, IReadOnlyDictionary<int, IReadOnlyList<string>> issues)
    {
        Paths = paths;
        Issues = issues;
    }

    public static CategoryPaths Build(IEnumerable<RecognitionCategory> categories)
    {
        var byId = new Dictionary<int, RecognitionCategory>();
        foreach (var category in categories)
        {
            if (!byId.TryAdd(category.Id, category))
                throw new InvalidDataException($"Duplicate category ID {category.Id}.");
        }

        var paths = new Dictionary<int, string>();
        var issues = new Dictionary<int, IReadOnlyList<string>>();
        foreach (var id in byId.Keys)
        {
            var labels = new List<string>();
            var found = new List<string>();
            var seen = new HashSet<int>();
            int? target = id;
            while (target is { } current)
            {
                if (!seen.Add(current))
                {
                    labels.Add("[цикл категорій]");
                    found.Add("category_cycle");
                    break;
                }

                if (!byId.TryGetValue(current, out var category))
                {
                    labels.Add($"[невідома категорія {current}]");
                    found.Add("category_parent_missing");
                    break;
                }

                labels.Add(string.IsNullOrEmpty(category.Name) ? current.ToString(CultureInfo.InvariantCulture) : category.Name);
                target = category.ParentId;
            }

            labels.Reverse();
            paths[id] = string.Join(" > ", labels);
            issues[id] = found;
        }

        return new CategoryPaths(paths, issues);
    }

    /// <summary>Paths known already (for example from the recognizer CSV), without a tree.</summary>
    public static CategoryPaths FromPaths(IReadOnlyDictionary<int, string> paths)
        => new(paths, paths.Keys.ToDictionary(id => id, _ => (IReadOnlyList<string>)[]));
}
