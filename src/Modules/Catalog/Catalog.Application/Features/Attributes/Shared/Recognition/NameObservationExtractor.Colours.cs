using System.Text.RegularExpressions;
using static Catalog.Application.Features.Attributes.Shared.Recognition.RecognitionPatterns;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>Colors in a name: regions, blocked and component colors, codes, and two-color combinations.</summary>
internal sealed partial class NameObservationExtractor
{
    private string ColourScope()
    {
        if (InEntry)
            return "entrance_door_finish";
        if (InInterior)
            return "interior_door_decor";
        return "hardware_finish";
    }

    private List<(int Start, int End)> ColourRegions()
    {
        if (IsSmart())
            return [];

        if (InEntry)
        {
            var dimension = PythonRegex.Search(Dimension, _name);
            if (dimension is null)
                return [];

            var tail = dimension.Index + dimension.Length;
            var side = PythonRegex.MatchStart(@"\s*[LR](?:\s+|$)", _name[tail..], ignoreCase: false);
            if (side is not null)
                tail += side.Length;
            var ending = PythonRegex.Search(@"\s*\+\s*2-й\s+замок", _name[tail..], ignoreCase: false);
            return [(tail, ending is not null ? tail + ending.Index : _name.Length)];
        }

        if (InInterior && PythonRegex.Search("^(?:Робота|Ремонт|Доставка)", _name) is not null)
            return [];

        return [(0, _name.Length)];
    }

    private void FindBlockedAndComponentColours()
    {
        foreach (Match match in PythonRegex.FindAll("\"Бронза\"|«Бронза»|білий\\s+цинк", _name))
            _blockedColour.Add(Span(match));

        if (PythonRegex.Search("дерев['’]яна", _name) is not null)
        {
            foreach (Match match in PythonRegex.FindAll(Bounded("Дуб|Вишня|Горіх"), _name))
            {
                Event("wood_species_or_decor", Span(match), status: RecognitionStatus.Unresolved, candidate: match.Value,
                    note: "У деревянной ручки порода и название отделки не различены. Не назначать автоматически цвет.");
                _blockedColour.Add(Span(match));
            }
        }

        foreach (Match match in PythonRegex.FindAll(@"біла\s+резинка", _name))
        {
            Event("component_color", Span(match), "WH", "Білий", scope: "rubber_part", note: "Цвет резинки, не всего упора.");
            _blockedColour.Add(Span(match));
        }

        (string Code, string Label, string Pattern)[] infill =
        [
            ("GLASS_BLACK", "Чорне скло", @"(?:скло|cкло)\s+чорне|Чорне\s+скло"),
            ("GLASS_WHITE", "Біле скло", @"скло\s+біле"),
            ("INFILL_SATIN_WHITE", "Сатин білий", @"Сатин\s+б[іi]лий"),
            ("INFILL_SATIN_BRONZE", "Сатин бронза", @"Сатин\s+бронза"),
            ("GLASS_BRONZE", "Бронзове скло", @"скло\s+бронза"),
            ("MIRROR_BRONZE_TRIPLEX", "Двостороннє бронзове триплекс-дзеркало", @"Двостороннє\s+бронзове\s+триплекс\s+дзеркало"),
            ("MIRROR_GRAPHITE_TRIPLEX", "Двостороннє графітове триплекс-дзеркало", @"Двостороннє\s+граф[іi]т\s+триплекс\s+дзеркало"),
            ("MIRROR_GRAPHITE", "Двостороннє графітове дзеркало", @"Двостороннє\s+граф[іi]т\s+дзеркало"),
            ("LACOBEL_BLACK", "Lacobel чорний лак", @"Lacobel\s+чорний\s+лак"),
            ("SOLID", "Глухе полотно", "глухе|глуха"),
            ("GLASS_SATIN", "Скло сатин", @"скло\s+сатин"),
        ];
        if (InInterior)
        {
            foreach (var (code, label, pattern) in infill)
            {
                foreach (Match match in PythonRegex.FindAll(Bounded(pattern), _name))
                {
                    if (Overlap(match.Index, match.Index + match.Length, _blockedColour))
                        continue;
                    Event("door_infill", Span(match), code, label, scope: "door_infill");
                    _blockedColour.Add(Span(match));
                }
            }

            foreach (Match match in PythonRegex.FindAll(Bounded(@"[CС]ірий\s+графіт|Сатин"), _name))
            {
                if (Overlap(match.Index, match.Index + match.Length, _blockedColour))
                    continue;
                Event("unassigned_finish", Span(match), status: RecognitionStatus.Unresolved,
                    scope: "interior_door_second_finish",
                    candidate: match.Value.ToLowerInvariant().Contains("графіт", StringComparison.Ordinal) ? "Сірий графіт" : "Сатин",
                    note: "Указано после модели/декора, но нет явного слова скло или названия компонента.");
                _blockedColour.Add(Span(match));
            }

            foreach (Match match in PythonRegex.FindAll(Bounded("BLK|М1"), _name))
            {
                Event("unresolved_marker", Span(match), status: RecognitionStatus.Unresolved, scope: "door_model_or_infill",
                    note: "Код после модели/декора: не назначать по нему чёрный цвет полотну.");
                _blockedColour.Add(Span(match));
            }
        }

        foreach (Match match in PythonRegex.FindAll(Bounded(Code("S") + @"\s*/\s*" + Code("CR")), _name))
        {
            Event("color", Span(match), status: RecognitionStatus.Unresolved, scope: ColourScope(),
                note: "S/CR в каталоге подписан как хром, но составной код нельзя уверенно приравнять к SN/CP.",
                sources: ["catalog_2020"], candidate: "Хром або сатин / хром");
            _blockedColour.Add(Span(match));
        }
    }

    private List<NameObservation> FindColours()
    {
        var regions = ColourRegions();
        var candidates = new List<(int Start, int End, ExtractionRule Rule, Match Match)>();
        foreach (var original in _dictionary.ColorRules)
        {
            var rule = original;
            if (rule.EligibleNamePattern is not null && PythonRegex.Search(rule.EligibleNamePattern, _name) is null)
            {
                rule = rule with
                {
                    Status = RecognitionStatus.Unresolved, NormalizedCode = null, NormalizedValueUk = null,
                    ScopeOverride = "unverified_brand_or_series_finish",
                    Note = "Расшифровка подобрана ассистентом для других проверенных серий; в этой серии значение кода не подтверждено."
                };
            }

            foreach (Match match in PythonRegex.FindAll(Bounded(rule.Pattern), _name))
            {
                var (start, end) = Span(match);
                if (!regions.Any(region => start >= region.Start && end <= region.End))
                    continue;
                if (Overlap(start, end, _blockedColour))
                    continue;
                if (SkipColourCode(rule, match, start, end))
                    continue;
                candidates.Add((start, end, rule, match));
            }
        }

        // Leftmost, longest match: no matching of white inside a named white decor.
        var selected = new List<(int Start, int End, ExtractionRule Rule, Match Match)>();
        foreach (var candidate in candidates
                     .OrderBy(item => item.Start)
                     .ThenBy(item => -(item.End - item.Start))
                     .ThenBy(item => item.Rule.Status != RecognitionStatus.Unresolved))
        {
            if (!Overlap(candidate.Start, candidate.End, selected.Select(item => (item.Start, item.End))))
                selected.Add(candidate);
        }

        var colourEvents = new List<NameObservation>();
        foreach (var (_, _, rule, match) in selected)
        {
            var observation = RuleEvent(rule, match, rule.ScopeOverride ?? ColourScope());
            if (PythonRegex.FullMatch(Code("CP"), match.Value) && _name.Contains("9011", StringComparison.Ordinal))
            {
                observation.NormalizedCode = null;
                observation.NormalizedValueUk = null;
                observation.Status = RecognitionStatus.Unresolved;
                observation.Scope = "lock_9011_finish";
                observation.Candidate = "Хром або нікель";
                observation.SourceRefs = ["catalog_2020"];
                observation.Note = "Контекст 9011: CP подписан в каталоге как никель, в других сериях как хром. Нужна проверка.";
            }

            if (PythonRegex.FullMatch(Code("BN"), match.Value))
            {
                if (InMechanism)
                {
                    observation.Scope = "mechanism_finish";
                }
                else
                {
                    observation.NormalizedCode = null;
                    observation.NormalizedValueUk = null;
                    observation.Status = RecognitionStatus.Unresolved;
                    observation.Scope = CategoryId == FzbCategoryId ? "FZB_finish" : "other_brand_finish";
                    observation.Candidate = "Чорний нікель";
                    observation.Note = "BN подтверждён словесной расшифровкой только в механизмах. Для FZB/другой серии смысл проверить.";
                }
            }

            if (PythonRegex.FullMatch(Code("NP"), match.Value))
            {
                observation.SourceRefs = ["export"];
                observation.Note = "NP (nickel) явно встречается у петли ID 8329. Уточнение о полировке не добавлялось.";
            }

            colourEvents.Add(observation);
        }

        // Known escutcheon model has an attached colour suffix in a few source names.
        foreach (var (suffix, code, label) in new[] { ("CP", "CR", "Хром"), ("GP", "G", "Золото"), ("AB", "AB", "Антична бронза") })
        {
            foreach (Match match in PythonRegex.FindAll($"(?<=[TТ]917){Code(suffix)}(?![{Letters}0-9])", _name))
            {
                colourEvents.Add(Event("color", Span(match), code, label, scope: "escutcheon_T917_suffix",
                    note: "Суффикс цвета слит с известной моделью Т917; правило ограничено этой моделью.",
                    sources: ["catalog_2020"]));
            }
        }

        // Context-specific abbreviations not safely reusable globally.
        foreach (Match match in PythonRegex.FindAll(Bounded("BL|WH|PC"), _name))
        {
            var (start, end) = Span(match);
            if (Overlap(start, end, _blockedColour) || Overlap(start, end, colourEvents.Select(e => (e.Start, e.End))))
                continue;
            if (IsSmart() || PythonRegex.MatchStart(@"-\d", _name[end..], ignoreCase: false) is not null)
                continue;

            var raw = match.Value.ToUpperInvariant();
            NameObservation observation;
            if (raw == "PC" && CategoryId == FzbCategoryId && PythonRegex.Search(@"\bFZB\b", _name) is not null)
            {
                observation = Event("color", Span(match), "CR", "Хром", scope: "FZB_finish",
                    note: "CR (PC) явно встречается у FZB. Нормализация ограничена FZB.");
            }
            else if (raw == "PC")
            {
                continue; // PC/РС is a model code outside the explicit FZB brand context.
            }
            else if (raw == "BL" && InMechanism)
            {
                observation = Event("color", Span(match), "MBP", "Чорний матовий", scope: "mechanism_finish",
                    note: "BL (Black Mat) явно расшифрован в названиях механизмов.");
            }
            else
            {
                observation = Event("color", Span(match), status: RecognitionStatus.Unresolved, scope: ColourScope(),
                    candidate: raw switch { "BL" => "Чорний", "WH" => "Білий", _ => "Хром" },
                    note: "Код вставки или покрытия требует подтверждения в этой серии; не переносить значение между контекстами.");
            }

            colourEvents.Add(observation);
        }

        // Homoglyph PC forms in FZB are genuine observed variants, kept exactly.
        foreach (Match match in PythonRegex.FindAll(Bounded(Code("PC")), _name))
        {
            if (CategoryId == FzbCategoryId && PythonRegex.Search(@"\bFZB\b", _name) is not null
                                            && !Overlap(match.Index, match.Index + match.Length, colourEvents.Select(e => (e.Start, e.End))))
            {
                colourEvents.Add(Event("color", Span(match), "CR", "Хром", scope: "FZB_finish",
                    note: "Смешанные алфавиты PC/РС, подтверждённый контекст FZB CR (PC)."));
            }
        }

        return colourEvents;
    }

    private bool SkipColourCode(ExtractionRule rule, Match match, int start, int end)
    {
        var rawKey = RecognitionDictionary.Folded(match.Value);
        var after = _name[end..];
        if (rule.Kind == "colour_code")
        {
            if (InInterior && PythonRegex.Search(@"Дверне\s+полотно|Каркасно-щитові\s+двері", _name) is not null)
            {
                var dimension = PythonRegex.Search(Dimension, _name);
                if (dimension is not null && start < dimension.Index + dimension.Length)
                    return true; // A coded model prefix before the leaf size is not its finish.
            }

            if (PythonRegex.MatchStart(@"-\d", after, ignoreCase: false) is not null)
                return true; // NP-02, SC-04 and other model prefixes are not colour codes.
            if (rawKey == "s" && (PythonRegex.Search(@"(?:\d+[BC]|PE\d+)-$", _name[..start]) is not null
                                  || PythonRegex.Search(@"Полицетримач\s+Пелікан\s+$", _name[..start]) is not null))
                return true; // 410B-S / 410C-S: model suffix.
            if (rawKey == "g" && PythonRegex.Search(@"^\s+(?:410[BC]-S|100\s+заскочка)", after) is not null)
                return true; // Series marker before a model.
            if (rawKey == "pb" && IsSmart())
                return true;
        }

        return rule.Kind == "unresolved_colour"
               && rawKey is "sb" or "ss" or "gr" or "ww" or "ac" or "orb"
               && PythonRegex.MatchStart(@"-\d", after, ignoreCase: false) is not null;
    }

    private void FindColourCombinations(List<NameObservation> colourEvents)
    {
        // Composite finish expressions are counted separately from their components.
        var ordered = colourEvents.OrderBy(e => e.Start).ToList();
        var sequences = new List<List<NameObservation>>();
        var current = new List<NameObservation>();
        foreach (var observation in ordered)
        {
            if (current.Count > 0)
            {
                var gap = _name[current[^1].End..observation.Start];
                if (!PythonRegex.FullMatch(@"\s*(?:\(UP\))?\s*[/\-]\s*", gap))
                {
                    if (current.Count > 1)
                        sequences.Add(current);
                    current = [];
                }
            }

            current.Add(observation);
        }

        if (current.Count > 1)
            sequences.Add(current);

        foreach (var sequence in sequences)
        {
            var known = sequence.All(e => e.IsApproved);
            Event("color_combination", (sequence[0].Start, sequence[^1].End),
                known ? string.Join("/", sequence.Select(e => e.NormalizedCode)) : null,
                known ? string.Join(" / ", sequence.Select(e => e.NormalizedValueUk)) : null,
                status: known ? RecognitionStatus.Approved : RecognitionStatus.Unresolved,
                scope: sequence.Any(item => item.Scope == "unverified_brand_or_series_finish")
                    ? "unverified_brand_or_series_finish"
                    : ColourScope(),
                note: "Порядок компонентов сохранён. Роли наружная/внутренняя, основание/вставка не назначены. У компонентов и комбинации пересекаются product_id.",
                sources: sequence.SelectMany(e => e.SourceRefs ?? []).Distinct().Order(StringComparer.Ordinal).ToList(),
                candidate: string.Join(" / ", sequence.Select(e => Or(e.NormalizedValueUk, e.Candidate, e.RawValue))),
                components: sequence.Select(e => e.AsComponent()).ToList());
        }

        for (var index = 0; index + 1 < ordered.Count; index++)
        {
            var (left, right) = (ordered[index], ordered[index + 1]);
            if (!PythonRegex.FullMatch(@"\s*\(\s*", _name[left.End..right.Start], ignoreCase: false))
                continue;
            var closing = PythonRegex.MatchStart(@"\s*\)", _name[right.End..], ignoreCase: false);
            if (closing is null)
                continue;

            var same = left.IsApproved && right.IsApproved
                       && (left.NormalizedCode == right.NormalizedCode
                           || (left.NormalizedCode, right.NormalizedCode) is ("MBP", "BLACK") or ("MWH", "WH") or ("ORB", "BLACK"));
            Event("color_alias_expression", (left.Start, right.End + closing.Length),
                same ? left.NormalizedCode : null, same ? left.NormalizedValueUk : null,
                status: same ? RecognitionStatus.Approved : RecognitionStatus.Unresolved, scope: ColourScope(),
                note: "Альтернативные обозначения в скобках, не два разных цвета. У отдельных вариантов и целого выражения общие product_id.",
                sources: (left.SourceRefs ?? []).Concat(right.SourceRefs ?? []).Distinct().Order(StringComparer.Ordinal).ToList());
        }
    }
}
