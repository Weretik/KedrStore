using System.Text.RegularExpressions;
using static Catalog.Application.Features.Attributes.Shared.Recognition.RecognitionPatterns;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>
/// Finds attribute values in one product name (Python <c>extract</c>): colors and their combinations, other text
/// values from the dictionary rules, sides, codes kept for review, units, and the product type. Every observation
/// keeps its exact position; the name itself is never changed.
/// </summary>
internal sealed partial class NameObservationExtractor
{
    private const int FzbCategoryId = 920000;
    private static readonly int[] MechanismCategoryIds = [6108, 2321];

    private readonly RecognitionProduct _product;
    private readonly RecognitionDictionary _dictionary;
    private readonly string _name;
    private readonly List<NameObservation> _events = [];
    private readonly List<(int Start, int End)> _blockedColour = [];

    public NameObservationExtractor(RecognitionProduct product, RecognitionDictionary dictionary)
    {
        _product = product;
        _dictionary = dictionary;
        _name = product.Name;
    }

    private int? CategoryId => _product.CategoryId;

    public static bool IsSmart(RecognitionProduct product, RecognitionDictionary dictionary)
        => (product.CategoryId is { } id && dictionary.SmartCategoryIds.Contains(id))
           || PythonRegex.Search(@"(?:циліндр|цилиндр)\s+(?:електрон{1,2}ий|электрон{1,2}ый)\s+L4-", product.Name) is not null;

    private bool IsSmart() => IsSmart(_product, _dictionary);

    private bool InEntry => CategoryId is { } id && _dictionary.EntryCategoryIds.Contains(id);
    private bool InInterior => CategoryId is { } id && _dictionary.InteriorCategoryIds.Contains(id);
    private bool InCylinder => CategoryId is { } id && _dictionary.CylinderCategoryIds.Contains(id);
    private bool InMechanism => CategoryId is { } id && MechanismCategoryIds.Contains(id);

    public List<NameObservation> Extract()
    {
        FindBlockedAndComponentColours();
        var colourEvents = FindColours();
        FindColourCombinations(colourEvents);
        FindOtherValues(colourEvents);
        FindSidesAndMarkers();
        FindUnitsAndType();
        return _events;
    }

    private NameObservation Event(string attribute, (int Start, int End) span, string? code = null, string? name = null,
        string status = RecognitionStatus.Approved, string scope = "general", string? note = "",
        IReadOnlyList<string>? sources = null, string? candidate = null, IReadOnlyList<ColorComponent>? components = null)
    {
        var (start, end) = span;
        if (start < 0 || start >= end || end > _name.Length)
            throw new InvalidOperationException($"Empty or invalid span {start}..{end} in product {_product.Id}.");

        var observation = new NameObservation
        {
            Attribute = attribute, Scope = scope, RawValue = _name[start..end], Start = start, End = end,
            NormalizedCode = code, NormalizedValueUk = name, Status = status, Note = note,
            SourceRefs = sources ?? ["export"], Candidate = candidate, Components = components
        };
        _events.Add(observation);
        return observation;
    }

    private static (int, int) Span(Match match) => (match.Index, match.Index + match.Length);

    private static (int, int) Span(Group group) => (group.Index, group.Index + group.Length);

    private NameObservation RuleEvent(ExtractionRule rule, Match match, string scope = "general", string? attribute = null)
        => Event(attribute ?? rule.Attribute, Span(match), rule.NormalizedCode, rule.NormalizedValueUk, rule.Status, scope,
            rule.Note, rule.SourceRefs, rule.Candidate);

    private void FindOtherValues(List<NameObservation> colourEvents)
    {
        var colourSpans = colourEvents.Select(e => (e.Start, e.End)).ToList();
        foreach (var attribute in _dictionary.OtherRules.Select(rule => rule.Attribute).Distinct().Order(StringComparer.Ordinal))
        {
            var found = new List<(Match Match, ExtractionRule Rule)>();
            foreach (var rule in _dictionary.OtherRules.Where(rule => rule.Attribute == attribute))
            {
                if (attribute == "seal_profile" && PythonRegex.Search("Ущільнювач", _name) is null)
                    continue;
                if (attribute == "latch_type" && PythonRegex.Search("механізм|міжкімнатний|заскочка|межком|мех-м", _name) is null)
                    continue;
                if (attribute == "construction" && rule.NormalizedCode == "BUTTERFLY_HINGE" && PythonRegex.Search("Завіса|петля", _name) is null)
                    continue;
                if (attribute == "guide_type" && PythonRegex.Search("Напрямні", _name) is null)
                    continue;

                foreach (Match match in PythonRegex.FindAll(Bounded(rule.Pattern), _name))
                {
                    if (attribute == "shape" && rule.NormalizedCode == "SQUARE" && PythonRegex.Search(@"^KEDR\s+Квадрат", _name) is not null)
                        continue;
                    if (attribute is "material" or "material_code")
                    {
                        if (Overlap(match.Index, match.Index + match.Length, colourSpans))
                            continue;
                        if (attribute == "material_code" && match.Value.ToLowerInvariant() == "цинк"
                                                         && PythonRegex.Search(@"білий\s+$", _name[..match.Index]) is not null)
                            continue;
                    }

                    found.Add((match, rule));
                }
            }

            var taken = new List<(int Start, int End)>();
            foreach (var (match, rule) in found.OrderBy(item => item.Match.Index).ThenBy(item => -item.Match.Length))
            {
                var (start, end) = Span(match);
                if (Overlap(start, end, taken))
                    continue;

                var scope = "general";
                if (attribute == "material")
                {
                    var after = _name.Substring(end, Math.Min(18, _name.Length - end));
                    var before = _name[Math.Max(0, start - 12)..start];
                    scope = PythonRegex.Search("ключ", after) is not null || PythonRegex.Search("ключ", before) is not null
                        ? "key_material"
                        : "mentioned_material_component_not_assigned";
                }

                RuleEvent(rule, match, scope);
                taken.Add((start, end));
            }
        }

        // Explicitly English key material; BRASS series name is not enough to assign it.
        foreach (Match match in PythonRegex.FindAll(Bounded(@"brass\s+key"), _name))
        {
            Event("key_material", Span(match), "BRASS", "Латунь", scope: "key_material",
                note: "Словесное brass key; не покрытие цилиндра.");
        }
    }

    private void FindSidesAndMarkers()
    {
        // Side lexicon with restricted contexts.
        var sideContext = InEntry || PythonRegex.Search(@"Завіса|петля|ручк|T5D|китай\.\s*двері", _name) is not null;
        if (sideContext)
        {
            var ambiguous = PythonRegex.Search(@"лів,\s*прав", _name);
            if (ambiguous is not null)
            {
                Event("side", Span(ambiguous), status: RecognitionStatus.Unresolved,
                    note: "В одной записи перечислены обе стороны, выбрать одну нельзя.");
            }

            foreach (Match match in PythonRegex.FindAll(Bounded(@"L|R|ліва|права|лів\.|прав\.|універсальний|універсальна|УНИВЕРСАЛЬНА"), _name))
            {
                if (ambiguous is not null && Overlap(match.Index, match.Index + match.Length, [Span(ambiguous)]))
                    continue;
                var raw = match.Value.ToLowerInvariant();
                var code = raw is "l" or "ліва" or "лів." ? "L" : raw is "r" or "права" or "прав." ? "R" : "UNIVERSAL";
                Event("side", Span(match), code, code switch { "L" => "Ліва", "R" => "Права", _ => "Універсальна" });
            }
        }

        // Cylinder composite codes remain intact. No unsupported suffix decomposition.
        if (InCylinder || PythonRegex.Search("циліндр|цилиндр", _name) is not null)
        {
            foreach (Match match in PythonRegex.FindAll($"(?<![{Letters}])Z[СC]?(?:AB|BM|N|G)?(?![{Letters}0-9])", _name))
            {
                Event("cylinder_configuration_code", Span(match), status: RecognitionStatus.Unresolved, scope: "mechanical_cylinder",
                    note: "Слитный код конфигурации/покрытия. Полная расшифровка владельцем ещё не подтверждена.");
            }

            foreach (Match match in PythonRegex.FindAll(@"(?<=\d)[TТ](?=\s*[*\/)]|$)", _name))
            {
                Event("cylinder_turn_marker", Span(match), status: RecognitionStatus.Unresolved, scope: "mechanical_cylinder",
                    candidate: "Поворотник", note: "Наличие и положение T/Т записаны буквально; семантика не подтверждена.");
            }
        }

        if (IsSmart())
        {
            foreach (Match match in PythonRegex.FindAll("(?<=L4-)(?:PB|SB|SR)", _name))
            {
                Event("smart_variant_code", Span(match), status: RecognitionStatus.Unresolved, scope: "SMART_L4",
                    note: "PB/SB/SR внутри L4 могут обозначать функциональный вариант, а не цвет. Цветовой словарь не применялся.");
            }
        }

        // Bearings and series markers.
        if (PythonRegex.Search("Завіса|петл", _name) is not null)
        {
            foreach (Match match in PythonRegex.FindAll(Bounded("B4|B2|2BB"), _name))
            {
                var isFour = match.Value.ToUpperInvariant() == "B4";
                Event("bearing_designation", Span(match), isFour ? "4" : null, isFour ? "4 підшипники" : null,
                    status: isFour ? RecognitionStatus.Approved : RecognitionStatus.Unresolved,
                    sources: isFour ? ["user"] : ["export"], candidate: isFour ? null : "2 підшипники",
                    note: isFour ? "B4 подтверждено пользователем." : "B2/2BB требуют подтверждения значения перед разметкой.");
            }
        }

        foreach (Match match in PythonRegex.FindAll(Bounded(@"G(?=\s+(?:410[BC]-S|100\s+заскочка))"), _name))
        {
            Event("series_marker", Span(match), status: RecognitionStatus.Unresolved,
                note: "G перед моделью механизма/защёлки. Не цвет G=золото.");
        }

        foreach (Match match in PythonRegex.FindAll(@"\(UP\)", _name))
        {
            Event("position_marker", Span(match), status: RecognitionStatus.Unresolved,
                note: "Цветовая комбинация известна, но пространственный смысл UP не подтвержден.");
        }

        // Model-like short codes deliberately not interpreted as colours.
        foreach (var (pattern, scope) in new[]
                 {
                     (@"\bHG\s*\d+\s+(L)\b", "padlock_shackle_code"), (@"\bLXCP\s*-\s*\d+\s*-\s*(L)\b", "padlock_variant"),
                     (@"\b1082\s+(L)\b", "lever_lock_variant")
                 })
        {
            foreach (Match match in PythonRegex.FindAll(pattern, _name))
            {
                Event("unresolved_marker", Span(match.Groups[1]), status: RecognitionStatus.Unresolved, scope: scope,
                    note: "Отдельная L в этой модели не доказана как сторона.");
            }
        }

        // Model prefixes ET/BK and embedded WC are inventoried without guessing their functions.
        var accessoryCodes = "(?:" + string.Join("|", new[] { "ET", "BK", "WC" }.Select(Code)) + ")";
        foreach (Match match in PythonRegex.FindAll(
                     $@"(?<![{Letters}]){accessoryCodes}(?=\s*[-\d]|\b)|(?<=D672)(?:BK|ET)|(?<=D80-23)(?:BK|ET)", _name))
        {
            if (_events.Any(e => e.Attribute == "function_code" && e.Start == match.Index))
                continue;
            Event("accessory_function_code", Span(match), status: RecognitionStatus.Unresolved,
                note: "Код внутри модели/серии аксессуара, значение функции требует подтверждения. Не является цветом.");
        }

        foreach (Match match in PythonRegex.FindAll("72/62|69/52", _name))
        {
            Event("unresolved_size_code", Span(match), status: RecognitionStatus.Unresolved,
                note: "Код геометрии/совместимости пока не расшифрован; числовые оси не назначались.");
        }
    }

    private static readonly Dictionary<string, (string Code, string Label)> UnitValues = new(StringComparer.Ordinal)
    {
        ["mm"] = ("MM", "мм"), ["мм"] = ("MM", "мм"), ["м"] = ("M", "м"), ["m"] = ("M", "м"), ["см"] = ("CM", "см"),
        ["кг"] = ("KG", "кг"), ["kg"] = ("KG", "кг"), ["н"] = ("N", "Н"), ["шт."] = ("PCS", "Штука"), ["шт"] = ("PCS", "Штука"),
        ["пар."] = ("PAIR", "Пара"), ["компл."] = ("SET", "Комплект"), ["кл."] = ("KEY", "Ключ"), ["кл"] = ("KEY", "Ключ"),
        ["ключів"] = ("KEY", "Ключ"), ["ключiв"] = ("KEY", "Ключ"), ["штук"] = ("PCS", "Штука"), ["градусів"] = ("DEG", "Градус"),
        ["градусiв"] = ("DEG", "Градус")
    };

    private void FindUnitsAndType()
    {
        // Unit dictionaries record just the literal unit following a number, not measurements.
        foreach (Match match in PythonRegex.FindAll(
                     @"(?<=\d)\s*(mm|мм|см|м|m|кг|kg|Н|шт\.?|штук|пар\.|компл\.|кл\.?|ключ[іi]в|градус[іi]в)(?![A-Za-zА-Яа-яІіЇїЄєҐґ])", _name))
        {
            var (code, label) = UnitValues[match.Groups[1].Value.ToLowerInvariant()];
            Event("measurement_unit", Span(match.Groups[1]), code, label, scope: "literal_unit_after_number",
                note: "Единица измерения/счёта из названия. Назначение самого числа не выводилось.");
        }

        // First explicit product-type mention, rather than all component nouns in a title.
        var typeMatches = new List<(int Start, int NegativeLength, Match Match, string Code, string Label)>();
        foreach (var (code, label, pattern) in _dictionary.ProductTypeRules)
        {
            foreach (Match match in PythonRegex.FindAll(Bounded(pattern), _name))
                typeMatches.Add((match.Index, -match.Length, match, code, label));
        }

        if (PythonRegex.Search(@"Ущільнювач\s+Стоєва", _name) is { } ambiguous)
        {
            Event("product_type", Span(ambiguous), status: RecognitionStatus.Unresolved, scope: "ambiguous_title_type",
                candidate: "Стоєва / ущільнювач",
                note: "В названии вместе УЩІЛЬНЮВАЧ и Стоєва. Не выбирать физический тип только по первому слову.");
        }
        else if (typeMatches.Count > 0)
        {
            var first = typeMatches.OrderBy(item => item.Start).ThenBy(item => item.NegativeLength).First();
            Event("product_type", Span(first.Match), first.Code, first.Label, scope: "first_explicit_type_mention",
                note: "Первое явное название вида изделия; это словарь типов, категории товаров не изменены.");
        }
    }

    private static string? Or(params string?[] values) => values.FirstOrDefault(value => !string.IsNullOrEmpty(value));
}
