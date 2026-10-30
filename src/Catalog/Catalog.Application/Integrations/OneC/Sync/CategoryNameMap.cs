using Catalog.Application.Integrations.OneC.Specifications;

namespace Catalog.Application.Integrations.OneC.Sync;

/// <summary>
/// Maps a 1C category name to an active category of the root. Names are trimmed and compared case-insensitively;
/// a name with no category or several categories does not resolve (R-023).
/// </summary>
public sealed class CategoryNameMap
{
    private const int MaxExamples = 10;

    private readonly Dictionary<string, List<int>> _idsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _missing = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _ambiguous = new(StringComparer.OrdinalIgnoreCase);

    public CategoryNameMap(IEnumerable<CategoryNameRow> categories)
    {
        foreach (var (id, name) in categories)
        {
            var key = Normalize(name);
            if (key.Length == 0)
                continue;

            if (!_idsByName.TryGetValue(key, out var ids))
                _idsByName[key] = ids = [];

            ids.Add(id);
        }
    }

    public int? Resolve(string? name)
    {
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

    private static string Normalize(string? name) => name?.Trim() ?? string.Empty;
}
