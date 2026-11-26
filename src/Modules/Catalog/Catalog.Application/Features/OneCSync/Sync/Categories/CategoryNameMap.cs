using Catalog.Application.Features.OneCSync.Source;

namespace Catalog.Application.Features.OneCSync.Sync.Categories;

/// <summary>
/// Maps the folder 1C gives a product to an active category of the root (R-023). 1C sends the folder as text, not as
/// a code, so it is matched against the folders 1C itself lists: first by the full path, then by the name. Our own
/// configured groups and manual categories are not 1C folders and never take part. Text is trimmed and compared
/// case-insensitively; a name of several folders, or of none, does not resolve. Without the folder list (1C gave
/// none) the names of the active categories are used as before.
/// </summary>
public sealed class CategoryNameMap
{
    private const int MaxExamples = 10;

    private readonly Dictionary<string, List<int>> _idsByPath = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<int>> _idsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _missing = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _ambiguous = new(StringComparer.OrdinalIgnoreCase);

    public CategoryNameMap(IEnumerable<CategoryNameRow> categories)
        : this(categories, [])
    {
    }

    public CategoryNameMap(IEnumerable<CategoryNameRow> activeCategories, IEnumerable<OneCCategoryDto> oneCFolders)
    {
        var active = activeCategories.ToList();
        var folders = oneCFolders.ToList();
        if (folders.Count == 0)
        {
            foreach (var (id, name) in active)
                Add(_idsByName, Normalize(name), id);
            return;
        }

        var activeIds = active.Select(category => category.Id).ToHashSet();
        foreach (var folder in folders.Where(folder => activeIds.Contains(folder.CategoryId))
                     .DistinctBy(folder => folder.CategoryId))
        {
            Add(_idsByPath, NormalizePath(folder.CategoryPath), folder.CategoryId);
            Add(_idsByName, Normalize(folder.CategoryName), folder.CategoryId);
        }
    }

    public int? Resolve(string? name)
    {
        if (_idsByPath.TryGetValue(NormalizePath(name), out var byPath) && byPath.Count == 1)
            return byPath[0];

        var key = Normalize(name);
        if (!_idsByName.TryGetValue(key, out var ids))
        {
            _missing.Add(key);
            return null;
        }

        if (ids.Count > 1)
        {
            _ambiguous.Add(key);
            return null;
        }

        return ids[0];
    }

    public IReadOnlyCollection<string> MissingNames => _missing;
    public IReadOnlyCollection<string> AmbiguousNames => _ambiguous;

    public void LogSummary(ILogger logger, string root)
    {
        if (_missing.Count == 0 && _ambiguous.Count == 0)
            return;

        logger.LogWarning(
            "Product category names for {Root} did not resolve. Missing: {MissingCount} [{MissingExamples}]; " +
            "ambiguous: {AmbiguousCount} [{AmbiguousExamples}]. Existing products keep their category; new ones are skipped.",
            root,
            _missing.Count, string.Join("; ", _missing.Take(MaxExamples)),
            _ambiguous.Count, string.Join("; ", _ambiguous.Take(MaxExamples)));
    }

    private static void Add(Dictionary<string, List<int>> index, string key, int id)
    {
        if (key.Length == 0)
            return;
        if (!index.TryGetValue(key, out var ids))
            index[key] = ids = [];
        if (!ids.Contains(id))
            ids.Add(id);
    }

    private static string Normalize(string? name) => name?.Trim() ?? string.Empty;

    private static string NormalizePath(string? path)
        => string.Join('/', (path ?? string.Empty).Split(['/', '\\'], StringSplitOptions.TrimEntries)
            .Where(segment => segment.Length > 0));
}
