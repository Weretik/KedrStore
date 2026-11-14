using static Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary.RecognitionNumbers;
using static Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary.RecognitionPatterns;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;
namespace Catalog.Application.Features.Attributes.Shared.Recognition.ProductNames;

internal sealed partial class ProductNameRecognition
{
    private static readonly HashSet<string> LinearRoles =
    [
        "center_distance", "plate_size", "key_length", "rod_length", "thickness", "explicit_length", "explicit_width",
        "diameter", "wing_dimensions", "dimensions", "section", "cylinder_parts", "diameter_tuple", "roll_length",
        "rail_length", "bolt_length", "screw_length", "nominal_size", "viewer_range"
    ];

    // Functional quantities are parsed before geometric dimensions.
    private static readonly (string Role, string Pattern)[] FunctionalPatterns =
    [
        ("center_distance", $@"(?:межось\.?|міжось\.?|межосьова|міжосьова)\s*(?:відстань\s*)?(?<v>{Number})\s*(?<u>мм|mm)?"),
        ("plate_size", $@"планка\s*(?<v>{Number})(?:\s*[*xх×]\s*(?<v2>{Number}))?\s*(?<u>мм|mm)"),
        ("key_length", $@"ключ\s+(?<v>{Number})\s*(?<u>мм|mm)"),
        ("rod_length", $@"шток\s+(?<v>{Number})\s*(?<u>мм|mm)"),
        ("thickness", $@"(?:товщина|толщина|товщ\.)\s*\(?\s*(?<v>{Number})(?:\s*[-–]\s*(?<v2>{Number}))?\s*(?<u>мм|mm)?"),
        ("explicit_length", $@"(?:довжина|довж\.|длина|дл\.)\s*(?<v>{Number})(?:\s*[-–]\s*(?<v2>{Number}))?\s*(?<u>мм|mm|см|cm|м|m)?"),
        ("explicit_width", $@"(?:шир\.|ширина)\s*(?<v>{Number})\s*(?<u>мм|mm|см|cm)?"),
        ("diameter", $@"(?:діаметр|диаметр|Ф|[dD])\s*(?<v>{Number})\s*(?<u>мм|mm)?"),
        ("weight", $@"(?<v>{Number})(?:\s*[-–]\s*(?<v2>{Number}))?\s*(?<u>кг|kg)"),
        ("force", $@"(?<v>{Number})\s*(?<u>Н|N)(?![{Letters}])"),
        ("keys", $@"(?<v>\d+)\s*(?:кл\.?|ключ[іi]в|ключей)(?![{Letters}])"),
        ("angle", $@"(?<v>{Number})\s*(?:градус[іi]в|градусів|°)")
    ];

    /// <summary>One size or quantity found in the name, with the numbers converted to millimetres where linear.</summary>
    private sealed class Measurement
    {
        public required string Role { get; init; }
        public required string Raw { get; init; }
        public required List<decimal> Values { get; init; }
        public string? Unit { get; init; }
        public List<decimal?>? ValuesMm { get; init; }
        public required int Start { get; init; }
        public required int End { get; init; }
        public bool ImplicitUnit { get; init; }
        public List<string?>? PartMarkers { get; set; }
        public List<string>? Axes { get; set; }

        public JsonObject ToJson()
        {
            var json = new JsonObject
            {
                ["role"] = Role, ["raw"] = Raw, ["values"] = JsonArray(Values), ["unit"] = Unit,
                ["values_mm"] = RecognitionNumbers.JsonArray(ValuesMm)
            };
            if (ImplicitUnit)
                json["unit_basis"] = "user_default_mm_when_omitted";
            if (PartMarkers is not null)
                json["part_markers"] = new JsonArray(PartMarkers.Select(marker => (JsonNode?)marker).ToArray());
            if (Axes is not null)
                json["axes"] = Strings(Axes);
            return json;
        }
    }

    private readonly List<Measurement> _measurements = [];
    private readonly List<(int Start, int End)> _occupied = [];

    private decimal? Millimetres(decimal value, string? unit)
    {
        if (unit is null || !_dictionary.UnitFactorsToMm.TryGetValue(unit.ToLowerInvariant(), out var factor))
            return null;
        return Normalize(value * factor);
    }

    private Measurement Add(string role, List<decimal> values, string? unit, int start, int end)
    {
        var implicitUnit = unit is null && LinearRoles.Contains(role);
        if (implicitUnit)
            unit = _dictionary.DefaultDimensionUnit;
        var measurement = new Measurement
        {
            Role = role, Raw = _name[start..end], Values = values, Unit = unit,
            ValuesMm = unit is not null && _dictionary.UnitFactorsToMm.ContainsKey(unit.ToLowerInvariant())
                ? values.Select(value => Millimetres(value, unit)).ToList()
                : null,
            Start = start, End = end, ImplicitUnit = implicitUnit
        };
        _measurements.Add(measurement);
        _occupied.Add((start, end));
        Drop(start, end);
        return measurement;
    }

    private Measurement Add(Match match, string role, List<decimal> values, string? unit)
        => Add(role, values, unit, match.Index, match.Index + match.Length);

    private static string? Optional(Match match, string group) => match.Groups[group].Success ? match.Groups[group].Value : null;

    private static JsonObject Range(List<decimal?> values) => new() { ["min"] = Json(values[0]), ["max"] = Json(values[1]) };

    private void Measurements()
    {
        FunctionalQuantities();
        WingDimensions();
        CompoundDimensions();
        DiameterTuples();
        SingleMeasurements();
        KindSpecificNumbers();
        Quantities();
        PickSize();
    }

    private void FunctionalQuantities()
    {
        foreach (var (role, pattern) in FunctionalPatterns)
        {
            foreach (Match match in PythonRegex.FindAll(pattern, _name))
            {
                var end = match.Index + match.Length;
                if (Overlap(match.Index, end, _occupied))
                    continue;
                if (end < _name.Length && PythonRegex.MatchStart($"[{Letters}0-9]", _name[end].ToString(), ignoreCase: false) is not null)
                    continue;

                var values = new List<decimal> { Parse(match.Groups["v"].Value) };
                if (Optional(match, "v2") is { } second)
                    values.Add(Parse(second));
                var unit = Optional(match, "u");
                // d/D/Ф are only diameter markers when not inside an alphanumeric model.
                if (role == "diameter" && match.Index > 0
                                       && PythonRegex.MatchStart($"[{Letters}0-9]", _name[match.Index - 1].ToString(), ignoreCase: false) is not null)
                    continue;
                if (role == "diameter" && PythonRegex.Search("^[dD]", match.Value, ignoreCase: false) is not null
                                       && _kind is not ("round_hinge" or "hinge_cap" or "other_hinge" or "metal_fitting"))
                    continue;

                var measure = Add(match, role, values, unit);
                var mm = measure.ValuesMm;
                switch (role)
                {
                    case "center_distance":
                        _extra["center_distance_mm"] = Json(mm![0]);
                        break;
                    case "plate_size":
                        _extra["plate_dimensions_mm"] = RecognitionNumbers.JsonArray(mm);
                        break;
                    case "key_length":
                        _extra["key_length_mm"] = Json(mm![0]);
                        break;
                    case "rod_length":
                        _extra["rod_length_mm"] = Json(mm![0]);
                        break;
                    case "thickness":
                        if (values.Count == 1)
                        {
                            Field("size_thickness", mm![0]);
                        }
                        else
                        {
                            _extra["thickness_range_mm"] = Range(mm!);
                            Inform("thickness_is_range", "Толщина указана диапазоном; size_thickness=null, диапазон сохранён отдельно.");
                        }

                        break;
                    case "explicit_length":
                        if (values.Count == 1)
                        {
                            Field("size_length", mm![0]);
                        }
                        else
                        {
                            _extra["length_range_mm"] = Range(mm!);
                            Inform("length_is_range", "Длина указана диапазоном; единственное значение не выбиралось.");
                        }

                        break;
                    case "explicit_width":
                        Field("size_width", mm![0]);
                        break;
                    case "diameter":
                        Field("size_diameter", mm![0]);
                        _extra["diameter_mm"] = Json(_row.SizeDiameter);
                        break;
                    case "weight":
                        _extra[_kind == "closer" ? "door_weight_kg" : "load_kg"] = values.Count > 1
                            ? new JsonObject { ["min"] = Json(values[0]), ["max"] = Json(values[^1]) }
                            : Json(values[0]);
                        if (_kind == "concealed_hinge")
                            _extra["load_applies_to"] = "Набір завіс, кількість з назви";
                        break;
                    case "force":
                        _extra["force_N"] = Json(values[0]);
                        break;
                    case "keys":
                        _extra["key_count"] = Json(values[0]);
                        break;
                    case "angle":
                        _extra["angle_degrees"] = Json(values[0]);
                        break;
                }
            }
        }
    }

    private void WingDimensions()
    {
        foreach (Match match in PythonRegex.FindAll($@"(?:крило|крилом)\s*\(?\s*(?<v>{Number})(?:\s*/\s*(?<v2>{Number}))?\s*(?<u>мм|mm)?", _name))
        {
            if (Overlap(match.Index, match.Index + match.Length, _occupied))
                continue;

            var values = new List<decimal> { Parse(match.Groups["v"].Value) };
            if (Optional(match, "v2") is { } second)
                values.Add(Parse(second));
            var measure = Add(match, "wing_dimensions", values, Optional(match, "u"));
            var last = match.Groups["u"].Success ? match.Groups["u"] : match.Groups["v2"].Success ? match.Groups["v2"] : match.Groups["v"];
            _remove[^1] = (match.Groups["v"].Index, last.Index + last.Length);
            _extra["wing_dimensions"] = new JsonObject
            {
                ["ordered_values"] = JsonArray(values), ["ordered_mm"] = RecognitionNumbers.JsonArray(measure.ValuesMm),
                ["unit"] = measure.Unit
            };
            if (values.Count > 1)
                Issue("wing_dimensions_unconfirmed", "У крыла несколько значений; их назначение не подтверждено.");
        }
    }

    private void CompoundDimensions()
    {
        // Compound dimensions: * / x / Cyrillic х / ×, with optional cylinder T markers.
        var pattern = $@"(?<![{Letters}0-9])(?<a>{Number})(?<ta>[TТ]?)\s*[*xх×]\s*(?<b>{Number})(?<tb>[TТ]?)(?:\s*[*xх×]\s*(?<c>{Number}))?\s*(?<u>мм|mm|см|cm)?";
        foreach (Match match in PythonRegex.FindAll(pattern, _name))
        {
            var (start, end) = (match.Index, match.Index + match.Length);
            if (Overlap(start, end, _occupied))
                continue;
            if (PythonRegex.Search(@"№\s*$", _name[Math.Max(0, start - 4)..start]) is not null)
                continue;

            var badUnit = PythonRegex.MatchStart(@"\s*(?:шт|уп|ящ|пар|компл)", _name[end..]);
            if (badUnit is not null)
            {
                if (_extra["invalid_dimensions"] is not JsonArray invalid)
                    _extra["invalid_dimensions"] = invalid = new JsonArray();
                invalid.Add(new JsonObject { ["raw"] = _name[start..(end + badUnit.Length)], ["normalized_value"] = null });
                Issue("invalid_dimension_unit", "В размерном выражении указана единица количества вместо линейной; значение не исправлялось.");
                continue;
            }

            var values = new List<decimal> { Parse(match.Groups["a"].Value), Parse(match.Groups["b"].Value) };
            if (Optional(match, "c") is { } third)
                values.Add(Parse(third));
            var role = _kind == "cylinder" ? "cylinder_parts" : _kind == "seal" ? "section" : "dimensions";
            var measure = Add(match, role, values, Optional(match, "u"));
            var ta = match.Groups["ta"].Value;
            var tb = match.Groups["tb"].Value;
            if (ta.Length > 0 || tb.Length > 0)
                measure.PartMarkers = [ta.Length > 0 ? ta : null, tb.Length > 0 ? tb : null];

            if (_kind == "flat_hinge")
            {
                measure.Axes = new[] { "length", "width", "thickness" }.Take(values.Count).ToList();
                if (measure.ValuesMm is { } converted)
                {
                    Field("size_length", converted[0]);
                    Field("size_width", converted[1]);
                    if (converted.Count == 3)
                        Field("size_thickness", converted[2]);
                }
            }
            else if (role == "cylinder_parts")
            {
                _extra["cylinder_parts"] = new JsonObject
                {
                    ["ordered_raw_values"] = JsonArray(values), ["ordered_mm"] = RecognitionNumbers.JsonArray(measure.ValuesMm),
                    ["turn_markers"] = measure.PartMarkers is null
                        ? null
                        : new JsonArray(measure.PartMarkers.Select(marker => (JsonNode?)marker).ToArray())
                };
                // Ordered halves are explicit dimensions, not an ambiguous axis assignment.
                measure.Axes = new[] { "part_a", "part_b" }.Take(values.Count).ToList();
            }
            else if (role == "section")
            {
                _extra["section_dimensions"] = new JsonObject
                {
                    ["ordered_values"] = JsonArray(values), ["ordered_mm"] = RecognitionNumbers.JsonArray(measure.ValuesMm)
                };
                Issue("section_axes_unconfirmed", "Извлечено сечение уплотнителя; назначение двух осей не подтверждено.");
            }
            else
            {
                Issue("dimension_axes_unconfirmed", "Размерное выражение извлечено, порядок длины/ширины/толщины не подтверждён.");
            }
        }
    }

    private void DiameterTuples()
    {
        // Diameter tuple D14*46: the preceding diameter has been captured separately.
        if (_kind is not ("round_hinge" or "hinge_cap"))
            return;

        foreach (Match match in PythonRegex.FindAll($@"(?:D|d|Ф)\s*{Number}\s*[*xх×]\s*(?<v>{Number})(?:\s*(?<u>мм|mm))?", _name))
        {
            if (_measurements.Any(m => m.Role == "diameter_tuple"))
                continue;
            var values = PythonRegex.FindAll(Number, match.Value, ignoreCase: false).Take(2).Select(m => Parse(m.Value)).ToList();
            var measure = Add(match, "diameter_tuple", values, Optional(match, "u"));
            measure.Axes = ["diameter", "length"];
            Field("size_diameter", measure.ValuesMm![0]);
            Field("size_length", measure.ValuesMm[1]);
        }
    }

    private void SingleMeasurements()
    {
        // Single measurements with explicit units, excluding functional quantities already claimed.
        foreach (Match match in PythonRegex.FindAll(
                     $@"(?<![{Letters}0-9])(?<v>{Number})(?:\s*[-–]\s*(?<v2>{Number}))?\s*(?<u>мм|mm|см|cm|м|m)(?![{Letters}0-9])", _name))
        {
            if (Overlap(match.Index, match.Index + match.Length, _occupied))
                continue;

            var values = new List<decimal> { Parse(match.Groups["v"].Value) };
            if (Optional(match, "v2") is { } second)
                values.Add(Parse(second));
            var unit = match.Groups["u"].Value;
            var role = _kind == "seal" && unit.ToLowerInvariant() is "м" or "m" ? "roll_length"
                : _kind == "rail" ? "rail_length"
                : "nominal_size";
            if (_kind == "bolt" && PythonRegex.Search(@"торцевий\s+" + PythonRegex.Escape(match.Value), _name) is not null)
                role = "bolt_length";
            if (_kind == "screw" && PythonRegex.Search(@"[MМ]\d+", _name, ignoreCase: false) is not null)
                role = "screw_length";

            var measure = Add(match, role, values, unit);
            if (role == "roll_length")
                _extra["roll_length_m"] = Json(values[0]);
            else if (role is "rail_length" or "bolt_length" or "screw_length" && values.Count == 1)
                Field("size_length", measure.ValuesMm![0]);
            else
                Issue("single_dimension_role_unconfirmed", "Линейный размер найден, но измеряемая часть/ось не подтверждена.");
        }
    }

    private void KindSpecificNumbers()
    {
        if (_kind == "viewer")
        {
            var match = PythonRegex.Search(@"(?<!\d)(\d+)\s*[-–]\s*(\d+)(?!\d)", _name, ignoreCase: false);
            if (match is not null)
            {
                var low = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var high = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                Add(match, "viewer_range", [low, high], null);
                _extra["viewer_range"] = new JsonObject
                {
                    ["raw"] = match.Value, ["min"] = Json(low), ["max"] = Json(high), ["meaning"] = null, ["unit"] = "mm"
                };
                Issue("viewer_range_unconfirmed", "Диапазон вічка сохранён в мм; его назначение не подтверждено.");
            }
        }

        if (_kind == "screw")
        {
            var match = PythonRegex.Search(@"[MМ](\d+)", _name, ignoreCase: false);
            if (match is not null)
            {
                _extra["thread_designation"] = "M" + match.Groups[1].Value;
                Drop(match.Index, match.Index + match.Length);
            }
        }
    }

    private void Quantities()
    {
        var quantities = new JsonArray();
        foreach (Match match in PythonRegex.FindAll(
                     $@"(?<![{Letters}0-9])(?<v>{Number})\s*(?<u>штука|штук|шт\.?|пар\.|компл\.?)(?![{Letters}])", _name))
        {
            if (Overlap(match.Index, match.Index + match.Length, _occupied))
                continue;
            if (PythonRegex.Search(@"[*xх×]\s*$", _name[..match.Index], ignoreCase: false) is not null)
                continue;

            var rawUnit = match.Groups["u"].Value.ToLowerInvariant();
            var unit = rawUnit.StartsWith("пар", StringComparison.Ordinal) ? "PAIR"
                : rawUnit.StartsWith("компл", StringComparison.Ordinal) ? "SET"
                : "PCS";
            quantities.Add(new JsonObject { ["value"] = Json(Parse(match.Groups["v"].Value)), ["unit"] = unit, ["raw"] = match.Value });
            Drop(match.Index, match.Index + match.Length);
        }

        if (quantities.Count > 0)
            _extra["quantities_from_name"] = quantities;

        // Explicit hinge counts and catalogue editions are not dimensions.
        var hinges = PythonRegex.Search(@"(\d+)\s*петл", _name);
        if (hinges is not null)
        {
            _extra["hinges_in_load_spec"] = int.Parse(hinges.Groups[1].Value, CultureInfo.InvariantCulture);
            if (_kind == "concealed_hinge")
            {
                var clause = PythonRegex.Search($@"\(\s*{Number}\s*(?:кг|kg)\s*[xх×]\s*\d+\s*петл\s*\)", _name);
                if (clause is not null)
                    Drop(clause.Index, clause.Index + clause.Length);
            }
        }

        if (_kind == "advertising" && PythonRegex.Search("Каталог", _name) is not null)
        {
            var year = PythonRegex.Search(@"\b(20\d{2})\b", _name, ignoreCase: false);
            if (year is not null)
            {
                _extra["edition_year"] = int.Parse(year.Groups[1].Value, CultureInfo.InvariantCulture);
                Drop(year.Index, year.Index + year.Length);
            }
        }
    }

    private void PickSize()
    {
        // A separate scalar component does not invalidate an unambiguous body tuple.
        // Its role can still need review; its raw value is retained in measurements.
        var primary = _measurements.Where(m => m.Role is "dimensions" or "section" or "cylinder_parts" or "diameter_tuple").ToList();
        if (primary.Count == 0)
            primary = _measurements.Where(m => m.Role is "nominal_size" or "bolt_length" or "screw_length" or "rail_length").ToList();

        if (primary.Count == 1)
        {
            var selected = primary[0];
            var markers = selected.PartMarkers;
            var components = selected.Values.Select((value, index) =>
                Format(value) + (markers is not null && index < markers.Count && markers[index] is { } marker ? marker : string.Empty));
            _row.Size = string.Join("×", components) + (selected.Unit is { } unit ? " " + unit.ToLowerInvariant() : string.Empty);
        }
        else if (primary.Count > 1)
        {
            Issue("multiple_size_expressions", "Несколько размерных выражений: размер сохранён в measurements, size и спорные оси оставлены null.");
            _row.Size = null;
            _row.SizeLength = null;
            _row.SizeWidth = null;
        }
        else
        {
            var body = _measurements.Where(m => m.Role is "explicit_length" or "explicit_width" or "thickness" or "diameter").ToList();
            if (_extra["invalid_dimensions"] is null && body.Count > 0)
            {
                var labels = new Dictionary<string, string>
                {
                    ["explicit_length"] = "довжина", ["explicit_width"] = "ширина", ["thickness"] = "товщина", ["diameter"] = "діаметр"
                };
                _row.Size = string.Join("; ", body.Select(m =>
                    labels[m.Role] + " " + string.Join("–", m.Values.Select(Format)) + " " + m.Unit!.ToLowerInvariant()));
            }
        }

        if (_measurements.Count > 0)
            _extra["measurements"] = new JsonArray(_measurements.Select(m => (JsonNode?)m.ToJson()).ToArray());

        if (_kind == "flat_hinge" && !_observations.Any(e => e.Attribute == "bearing_designation"))
            _extra["bearing_count"] = null;
    }
}
