using System.Text.Json.Nodes;
using static Catalog.Application.Features.Attributes.Shared.Recognition.RecognitionPatterns;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

internal sealed partial class ProductNameRecognition
{
    private void Packaging()
    {
        var stack = new Stack<int>();
        var pairs = new List<(int Start, int End)>();
        for (var index = 0; index < _name.Length; index++)
        {
            if (_name[index] == '(')
            {
                stack.Push(index);
            }
            else if (_name[index] == ')' && stack.Count > 0)
            {
                var start = stack.Pop();
                if (stack.Count == 0)
                    pairs.Add((start, index + 1));
            }
        }

        if (stack.Count > 0 || _name.Count(character => character == '(') != _name.Count(character => character == ')'))
            Inform("unbalanced_parentheses", "В исходном названии несбалансированные скобки; original_name сохранён без исправления.");

        var packages = new List<string>();
        foreach (var (start, end) in pairs)
        {
            var raw = _name[start..end];
            if (PythonRegex.Search(@"шт|уп\.?|ящ|пар\.|пара|компл", raw) is null)
                continue;

            var remainder = PythonRegex.Replace(
                @"штука|штук|шт|упаковка|упаковки|уп\.?|ящик|ящ\.?|пачка|мішок|пара|пар\.|комплект|компл\.|рулон", raw, "",
                ignoreCase: true);
            remainder = PythonRegex.Replace(@"[\d\s(),.*xх×/\\\-]+", remainder, "");
            if (PythonRegex.Search($"[{Letters}]", remainder, ignoreCase: false) is not null)
                continue;

            packages.Add(raw);
            Drop(start, end);
            if (PythonRegex.Search(@"\d+\s*\(\s*\d+|\d+\s*/\s*\d+", raw) is not null)
                Issue("packaging_alternatives", "В упаковке указаны альтернативные количества; одно значение не выбиралось.");
        }

        if (packages.Count > 0)
            _extra["packaging_raw"] = Strings(packages);
    }

    private void BaseName()
    {
        var characters = _name.ToCharArray();
        var removed = new HashSet<int>();
        foreach (var (start, end) in _remove)
        {
            for (var index = start; index < end; index++)
            {
                characters[index] = ' ';
                removed.Add(index);
            }
        }

        // Hyphens adjoining an extracted span are attribute delimiters. Preserve
        // model hyphens such as PR - 01 and ZCG-40KGS, which do not adjoin a span.
        for (var index = 0; index < _name.Length; index++)
        {
            if (_name[index] is not ('-' or '–' or '—'))
                continue;
            var left = index - 1;
            var right = index + 1;
            while (left >= 0 && char.IsWhiteSpace(_name[left]))
                left--;
            while (right < _name.Length && char.IsWhiteSpace(_name[right]))
                right++;
            if (removed.Contains(left) || removed.Contains(right))
                characters[index] = ' ';
        }

        var text = new string(characters);
        // Empty quote pairs can remain after removal of an attribute in quotes.
        foreach (var pattern in new[] { "\"([^\"\\n]*)\"", "'([^'\\n]*)'", "«([^»\\n]*)»", "“([^”\\n]*)”", "„([^“\\n]*)“" })
            text = PythonRegex.Replace(pattern, text, match => match.Groups[1].Value.Trim().Length == 0 ? " " : match.Value);

        for (var round = 0; round < 4; round++)
        {
            text = PythonRegex.Replace(@"\(\s*[,/\\\-]*\s*\)", text, " ");
            text = PythonRegex.Replace(@"[,;]\s*(?=[,;)]|$)", text, " ");
        }

        const string trimmed = " \t-_,;/*&\\";
        text = PythonRegex.Replace(@"(?<=\s)[,;]+|[,;]+(?=\s*$)", text, " ");
        text = PythonRegex.Replace(@"(?<=\s)[+*/&\\]+(?=\s|$)", text, " ");
        text = PythonRegex.Replace(@"\s*-\s*(?=[),;]|$)", text, " ");
        text = PythonRegex.Replace(@"(?<=\s)-(?=(?:\d+BB|B\d+)\b)", text, "");
        text = PythonRegex.Replace(@"\s+", text, " ").Trim(trimmed.ToCharArray());
        text = PythonRegex.Replace(@"(?<=\s)[.,;]+(?=\s*[),]|$)", text, "");
        text = PythonRegex.Replace(@"[,;]\s*(?=[.,;)]|$)", text, "");
        text = PythonRegex.Replace(@"\s+", text, " ");
        text = PythonRegex.Replace(@"\s+([,)])", text, "$1");
        text = PythonRegex.Replace(@"\(\s+", text, "(");
        text = PythonRegex.Replace(@"(?:\s+-\s*){2,}", text, " ");
        text = PythonRegex.Replace(@"\(\s*[-–—]+\s*", text, "(");
        text = PythonRegex.Replace(@"\s+", text, " ").Trim(" \t-–—_,;/*&\\".ToCharArray());

        _row.BaseName = text.Length > 0 ? text : null;
        if (_extra["unresolved_attributes"] is not null)
        {
            _extra["base_name_policy"] =
                "Нерасшифрованные модельные/служебные коды сохранены; это буквальная основа, не идентификатор общей модели.";
        }
    }
}
