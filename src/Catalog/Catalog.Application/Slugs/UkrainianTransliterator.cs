using System.Text;

namespace Catalog.Application.Slugs;

/// <summary>
/// Ukrainian national transliteration (Cabinet of Ministers resolution No. 55, 2010),
/// plus the few Russian letters that appear in 1C names.
/// </summary>
public static class UkrainianTransliterator
{
    private static readonly Dictionary<char, string> Letters = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "h", ['ґ'] = "g", ['д'] = "d", ['е'] = "e",
        ['є'] = "ie", ['ж'] = "zh", ['з'] = "z", ['и'] = "y", ['і'] = "i", ['ї'] = "i", ['й'] = "i",
        ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r",
        ['с'] = "s", ['т'] = "t", ['у'] = "u", ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch",
        ['ш'] = "sh", ['щ'] = "shch", ['ь'] = "", ['ю'] = "iu", ['я'] = "ia",
        // Russian letters that are not part of the Ukrainian alphabet.
        ['ы'] = "y", ['э'] = "e", ['ё'] = "io", ['ъ'] = ""
    };

    private static readonly Dictionary<char, string> WordInitial = new()
    {
        ['є'] = "ye", ['ї'] = "yi", ['й'] = "y", ['ю'] = "yu", ['я'] = "ya", ['ё'] = "yo"
    };

    private static readonly HashSet<char> Apostrophes = ['\'', '’', 'ʼ', '`', '‘'];

    public static string Transliterate(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var source = text.ToLowerInvariant();
        var builder = new StringBuilder(source.Length * 2);
        var wordStart = true;

        for (var i = 0; i < source.Length; i++)
        {
            var current = source[i];

            if (Apostrophes.Contains(current))
                continue;

            if (current == 'з' && i + 1 < source.Length && source[i + 1] == 'г')
            {
                builder.Append("zgh");
                i++;
                wordStart = false;
                continue;
            }

            if (wordStart && WordInitial.TryGetValue(current, out var initial))
                builder.Append(initial);
            else if (Letters.TryGetValue(current, out var latin))
                builder.Append(latin);
            else
                builder.Append(current);

            wordStart = !char.IsLetterOrDigit(current);
        }

        return builder.ToString();
    }
}
