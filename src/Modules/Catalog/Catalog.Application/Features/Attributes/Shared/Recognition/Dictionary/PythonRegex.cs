using System.Collections.Concurrent;
using System.Text.RegularExpressions;
namespace Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;

/// <summary>
/// The recognizer was written in Python (<c>docs/catalog-attributes/extract_attributes.py</c>); its patterns use
/// Python <c>re</c> calls. This keeps their meaning: <c>search</c>, <c>match</c> (only at the start),
/// <c>fullmatch</c>, and <c>finditer</c>, case-insensitive (<c>re.I</c>) or not. Regexes are compiled once.
/// </summary>
internal static class PythonRegex
{
    private static readonly ConcurrentDictionary<(string Pattern, bool IgnoreCase, char Mode), Regex> Cache = new();

    public static Regex Of(string pattern, bool ignoreCase = true) => Get(pattern, ignoreCase, 's');

    /// <summary><c>re.search</c>: the first match anywhere, or null.</summary>
    public static Match? Search(string pattern, string text, bool ignoreCase = true)
        => Found(Get(pattern, ignoreCase, 's').Match(text));

    /// <summary><c>re.match</c>: a match that starts at the beginning of the text, or null.</summary>
    public static Match? MatchStart(string pattern, string text, bool ignoreCase = true)
        => Found(Get(pattern, ignoreCase, 'm').Match(text));

    /// <summary><c>re.fullmatch</c>: whether the whole text matches.</summary>
    public static bool FullMatch(string pattern, string text, bool ignoreCase = true)
        => Get(pattern, ignoreCase, 'f').IsMatch(text);

    /// <summary><c>re.finditer</c>.</summary>
    public static IEnumerable<Match> FindAll(string pattern, string text, bool ignoreCase = true)
        => Get(pattern, ignoreCase, 's').Matches(text);

    public static string Replace(string pattern, string text, string replacement, bool ignoreCase = false)
        => Get(pattern, ignoreCase, 's').Replace(text, replacement);

    public static string Replace(string pattern, string text, MatchEvaluator evaluator, bool ignoreCase = false)
        => Get(pattern, ignoreCase, 's').Replace(text, evaluator);

    /// <summary>Python <c>re.escape</c> for a literal inside a pattern.</summary>
    public static string Escape(string text) => Regex.Escape(text);

    private static Match? Found(Match match) => match.Success ? match : null;

    private static Regex Get(string pattern, bool ignoreCase, char mode)
        => Cache.GetOrAdd((pattern, ignoreCase, mode), static key =>
        {
            var body = key.Pattern.Replace("(?P<", "(?<", StringComparison.Ordinal);
            body = key.Mode switch
            {
                'm' => $@"\G(?:{body})",
                'f' => $@"\A(?:{body})\z",
                _ => body
            };
            var options = RegexOptions.CultureInvariant | (key.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            return new Regex(body, options);
        });
}
