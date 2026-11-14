namespace Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;

/// <summary>Pattern building blocks shared by the recognizer stages (Python module constants and helpers).</summary>
internal static class RecognitionPatterns
{
    public const string Letters = "A-Za-zА-Яа-яІіЇїЄєҐґ";
    public const string Number = @"\d+(?:[.,]\d+)?";
    public static readonly string Dimension = $@"{Number}\s*[*xх×]\s*{Number}";

    private static readonly Dictionary<char, string> Homoglyphs = new()
    {
        ['A'] = "[AА]", ['B'] = "[BВ]", ['C'] = "[CС]", ['E'] = "[EЕ]", ['H'] = "[HН]", ['K'] = "[KК]",
        ['M'] = "[MМ]", ['O'] = "[OО]", ['P'] = "[PР]", ['T'] = "[TТ]", ['X'] = "[XХ]"
    };

    /// <summary>The pattern only where it is not glued to letters or digits.</summary>
    public static string Bounded(string pattern) => $"(?<![{Letters}0-9])(?:{pattern})(?![{Letters}0-9])";

    /// <summary>A Latin code that also matches the Cyrillic letters that look the same.</summary>
    public static string Code(string text)
        => string.Concat(text.Select(character => Homoglyphs.TryGetValue(character, out var pattern)
            ? pattern
            : PythonRegex.Escape(character.ToString())));

    public static bool Overlap(int start, int end, IEnumerable<(int Start, int End)> spans)
        => spans.Any(span => start < span.End && span.Start < end);
}
