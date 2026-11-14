using System.Text.Json.Nodes;
using static Catalog.Application.Features.Attributes.Shared.Recognition.RecognitionPatterns;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

internal sealed partial class ProductNameRecognition
{
    /// <summary>
    /// Finds the observations and decodes them with the dictionary (Python <c>Dictionary.recognize</c>). A spelling the
    /// owner decided means nothing is dropped.
    /// </summary>
    private List<NameObservation> Recognize()
    {
        var observations = new NameObservationExtractor(_product, _dictionary).Extract();
        var ignored = new HashSet<NameObservation>();
        foreach (var observation in observations)
        {
            var entry = _dictionary.Find(observation.Attribute, observation.Scope, observation.RawValue,
                _product.CategoryId);
            if (entry?.EligibleNamePattern is { } eligible && PythonRegex.Search(eligible, _name) is null)
                entry = null;

            if (entry is not null)
            {
                _row.FoundCodes.Add((observation.Attribute, observation.Scope,
                    RecognitionDictionary.Folded(observation.RawValue)));
                if (entry.IsIgnored)
                {
                    ignored.Add(observation);
                    continue;
                }

                observation.Status = entry.Status;
                observation.NormalizedCode = entry.NormalizedCode;
                observation.NormalizedValueUk = entry.NormalizedValueUk;
                observation.Note = entry.Note;
                observation.SourceRefs = entry.SourceRefs;
                observation.SelectedBy = entry.SelectedBy;
            }
            else if (observation.IsApproved)
            {
                var allowed = _dictionary.IsAllowed(observation.Attribute, observation.NormalizedCode, observation.NormalizedValueUk);
                if (observation.Attribute == "color_combination" && _dictionary.AllowKnownComponentCombinations)
                    allowed = (observation.Components ?? []).All(component => component.Status == RecognitionStatus.Approved);
                if (!allowed)
                {
                    observation.Status = RecognitionStatus.Unresolved;
                    observation.NormalizedCode = null;
                    observation.NormalizedValueUk = null;
                    observation.Note = "Новое значение не имеет утверждённого соответствия в словаре.";
                }
            }
        }

        observations.RemoveAll(ignored.Contains);

        // Atomic mappings may have changed since the original composite was inventoried.
        // Rebuild components from their positions, preserving unknown components and source order.
        foreach (var combination in observations.Where(item => item.Attribute == "color_combination"))
        {
            var atoms = observations
                .Where(item => item.Attribute == "color" && combination.Start <= item.Start && item.End <= combination.End)
                .OrderBy(item => item.Start)
                .ToList();
            if (atoms.Count < 2)
                continue;

            combination.Components = atoms.Select(item => item.AsComponent()).ToList();
            var known = atoms.All(item => item.IsApproved);
            combination.Status = known ? RecognitionStatus.Approved : RecognitionStatus.Unresolved;
            combination.NormalizedCode = known ? string.Join("/", atoms.Select(item => item.NormalizedCode)) : null;
            combination.NormalizedValueUk = known ? string.Join(" / ", atoms.Select(item => item.NormalizedValueUk)) : null;
        }

        return observations;
    }

    private void Lexical()
    {
        _observations = Recognize();
        var attributes = new SortedDictionary<string, List<(string? Code, string? Name, string Scope, List<string> Raw)>>(StringComparer.Ordinal);
        var unresolved = new JsonArray();
        foreach (var observation in _observations)
        {
            var attribute = observation.Attribute;
            if (!observation.IsApproved)
            {
                unresolved.Add(new JsonObject
                {
                    ["attribute"] = attribute, ["raw"] = observation.RawValue, ["code"] = null, ["name_uk"] = null,
                    ["reason"] = string.IsNullOrEmpty(observation.Note) ? "Неоднозначное значение" : observation.Note
                });
                Issue("unresolved_" + attribute,
                    $"«{observation.RawValue}»: {(string.IsNullOrEmpty(observation.Note) ? "соответствие не подтверждено" : observation.Note)}");
            }
            else if (attribute is not ("color" or "color_combination" or "color_alias_expression" or "measurement_unit" or "side"))
            {
                if (!attributes.TryGetValue(attribute, out var items))
                    attributes[attribute] = items = [];
                var item = items.FirstOrDefault(value => value.Code == observation.NormalizedCode
                                                         && value.Name == observation.NormalizedValueUk
                                                         && value.Scope == observation.Scope);
                if (item.Raw is null)
                {
                    item = (observation.NormalizedCode, observation.NormalizedValueUk, observation.Scope, []);
                    items.Add(item);
                }

                if (!item.Raw.Contains(observation.RawValue))
                    item.Raw.Add(observation.RawValue);
            }

            if (RemovedAttributes.Contains(attribute)
                && (observation.IsApproved || attribute is "color" or "color_combination" or "color_alias_expression" or "bearing_designation" or "side"))
            {
                // Keep unknown model markers. Identified colour roles may be removed even if their decoding is unknown.
                var start = observation.Start;
                var prefix = PythonRegex.MatchStart(@"Super\s+PET\s+", observation.RawValue);
                if (prefix is not null)
                    start += prefix.Length;
                Drop(start, observation.End);
            }
        }

        if (attributes.Count > 0)
        {
            var json = new JsonObject();
            foreach (var (attribute, items) in attributes)
            {
                json[attribute] = new JsonArray(items.Select(item => (JsonNode?)new JsonObject
                {
                    ["code"] = item.Code, ["name_uk"] = item.Name, ["scope"] = item.Scope, ["raw"] = Strings(item.Raw)
                }).ToArray());
            }

            _extra["attributes"] = json;
        }

        if (unresolved.Count > 0)
            _extra["unresolved_attributes"] = unresolved;
        if (_observations.Any(e => e.Attribute == "branding") && PythonRegex.Search(@"\bKEDR\b|\bCLASS\b", _name) is not null)
            Issue("brand_marking_conflict", "В названии одновременно указан бренд и No Name; исходные сведения сохранены без выбора одного варианта.");

        // Side remains null unless explicitly stated and unambiguous.
        var sideEvents = _observations.Where(e => e.Attribute == "side").ToList();
        var knownSides = sideEvents.Where(e => e.IsApproved).Select(e => e.NormalizedCode).Distinct().ToList();
        if (sideEvents.Any(e => !e.IsApproved) || knownSides.Count > 1)
            Issue("ambiguous_side", "Сторона неоднозначна; side=null.");
        else if (knownSides.Count == 1)
            _row.Side = knownSides[0];

        // Resolved bearing counts are separated from the literal designation.
        var bearingEvents = _observations.Where(e => e.Attribute == "bearing_designation").ToList();
        if (bearingEvents.Count > 0)
        {
            var counts = bearingEvents
                .Where(e => e.IsApproved && e.NormalizedCode is { Length: > 0 } code && code.All(char.IsDigit))
                .Select(e => int.Parse(e.NormalizedCode!, CultureInfo.InvariantCulture)).Distinct().ToList();
            _extra["bearing_count"] = counts.Count == 1 && bearingEvents.All(e => e.IsApproved) ? counts[0] : null;
        }

        if (_kind is "flat_hinge" or "other_hinge")
        {
            foreach (var match in PythonRegex.FindAll(Bounded(@"B\d+|\d+BB"), _name))
            {
                if (_observations.Any(e => e.Attribute == "bearing_designation" && e.Start == match.Index))
                    continue;
                _extra["bearing_count"] = null;
                Issue("unknown_bearing_designation", $"Неизвестное обозначение подшипников «{match.Value}».");
            }

            // A code in the explicit finish slot of a flat hinge is a found value,
            // even when it is new to the dictionary. Absence of that slot is harmless.
            if (_kind == "flat_hinge")
            {
                var slot = PythonRegex.Search(
                    $@"{Number}\s*[*xх×]\s*{Number}(?:\s*[*xх×]\s*{Number})?(?:mm|мм)?(?:\s*-\s*(?:B\d+|\d+BB))?\s*-\s*(?<code>[A-Za-z]{{2,20}})(?![A-Za-z0-9])",
                    _name);
                if (slot is not null)
                {
                    var code = slot.Groups["code"];
                    if (!Overlaps(code.Index, code.Index + code.Length, _observations.Select(item => (item.Start, item.End))))
                    {
                        if (_extra["unresolved_attributes"] is not JsonArray list)
                            _extra["unresolved_attributes"] = list = new JsonArray();
                        list.Add(new JsonObject
                        {
                            ["attribute"] = "color", ["raw"] = code.Value, ["code"] = null, ["name_uk"] = null,
                            ["reason"] = "Новый код в позиции цвета плоской петли."
                        });
                        _row.ColorRaw = code.Value;
                        Drop(code.Index, code.Index + code.Length);
                        Issue("unresolved_color", $"Неизвестный код цвета «{code.Value}» в позиции после размера петли.");
                    }
                }
            }
        }

        // Opening method descriptions form one variant clause of L4 smart cylinders.
        if (_kind == "smart_cylinder")
        {
            var clause = PythonRegex.Search(@"\s+з\s+(?:паролем|відкриттям)\b", _name);
            if (clause is not null && _observations.Any(e => e.Attribute == "opening_method"))
                Drop(clause.Index, _name.Length);
        }
    }

    private void Colors()
    {
        var values = _observations.Where(e => e.Attribute == "color").ToList();
        var compounds = _observations.Where(e => e.Attribute is "color_combination" or "color_alias_expression").ToList();
        if (values.Count == 0)
            return;

        var all = values.Concat(compounds).ToList();
        var start = all.Min(e => e.Start);
        var end = all.Max(e => e.End);
        _row.ColorRaw = _name[start..end];
        _extra["color_mentions"] = new JsonArray(values.Select(e => (JsonNode?)new JsonObject
        {
            ["raw"] = e.RawValue, ["code"] = e.NormalizedCode, ["name_uk"] = e.NormalizedValueUk, ["status"] = e.Status,
            ["source_refs"] = e.SourceRefs is null ? null : Strings(e.SourceRefs), ["selected_by"] = e.SelectedBy
        }).ToArray());

        var combinations = compounds.Where(e => e.Attribute == "color_combination").ToList();
        if (combinations.Count == 1)
        {
            var chosen = combinations[0];
            if (values.All(e => chosen.Start <= e.Start && e.End <= chosen.End))
            {
                var components = chosen.Components ?? [];
                _extra["color_components"] = new JsonArray(components.Select(c => (JsonNode?)Component(c)).ToArray());
                if (components.Count > 0 && components[0].Status == RecognitionStatus.Approved)
                {
                    _row.ColorCode = components[0].NormalizedCode;
                    _row.ColorName = components[0].NormalizedValueUk;
                }

                if (components.Count > 1 && components[1].Status == RecognitionStatus.Approved)
                {
                    _row.ColorSecondaryCode = components[1].NormalizedCode;
                    _row.ColorSecondaryName = components[1].NormalizedValueUk;
                }

                _extra["color_order_policy"] =
                    "Основной — первый компонент в названии; второй — следующий. Физическая роль сторон/вставок не предполагается.";
                if (components.Count > 2)
                    _extra["additional_color_components"] = new JsonArray(components.Skip(2).Select(c => (JsonNode?)Component(c)).ToArray());
                return;
            }
        }

        if (all.Any(e => !e.IsApproved))
            return;

        var codes = values.Select(e => (e.NormalizedCode, e.NormalizedValueUk)).Distinct().ToList();
        if (codes.Count == 1)
        {
            (_row.ColorCode, _row.ColorName) = codes[0];
            return;
        }

        // MBP (чорний) is a compatible general description of a more specific finish.
        var aliases = compounds.Where(e => e.Attribute == "color_alias_expression" && e.Start <= start && end <= e.End).ToList();
        if (aliases.Count == 1)
        {
            _row.ColorCode = aliases[0].NormalizedCode;
            _row.ColorName = aliases[0].NormalizedValueUk;
        }
        else
        {
            Issue("multiple_unassigned_colors", "Несколько цветов без однозначной связи; color_code/name=null.");
        }
    }
}
