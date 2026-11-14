using System.Text.Json;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>A regex rule of the dictionary (<c>extraction_rules</c>) that finds an attribute value in a name.</summary>
internal sealed record ExtractionRule(
    string Attribute, string? NormalizedCode, string? NormalizedValueUk, string Pattern, string Status, string? Note,
    IReadOnlyList<string> SourceRefs, string? Candidate, string? Kind, string? EligibleNamePattern)
{
    /// <summary>Set when the rule does not apply to this brand or series.</summary>
    public string? ScopeOverride { get; init; }
}

/// <summary>A known spelling (<c>entries</c>): attribute, scope, and text as written → its decoding.</summary>
internal sealed record DictionaryEntry(
    string Attribute, string Scope, string RawValue, string Status, string? NormalizedCode, string? NormalizedValueUk,
    string? Note, IReadOnlyList<string> SourceRefs, string? SelectedBy, string? EligibleNamePattern)
{
    /// <summary>The owner decided the code means nothing: the recognizer ignores what it found.</summary>
    public bool IsIgnored { get; init; }
}

/// <summary>
/// A person's decision in «Не розшифровані коди»: the value the spelling means, or none; for all products, or only for
/// products of <c>CategoryId</c> (spec catalog/008), which wins over the decision for all.
/// </summary>
public sealed record AttributeCodeDecision(
    string Attribute, string Scope, string RawValue, string? ValueCode, string? ValueNameUk, int? CategoryId = null);

/// <summary>
/// The recognizer dictionary: regex rules, known spellings, and parser settings from
/// <c>docs/catalog-attributes/dictionaries.json</c>, with the owner's code decisions on top.
/// </summary>
public sealed class RecognitionDictionary
{
    private const string ApprovalStatus = "supported_mappings_approved_unresolved_remain_null";

    private readonly Dictionary<(string, string, string), DictionaryEntry> _entries = new();
    private readonly Dictionary<(string, string, string), DictionaryEntry> _foldedEntries = new();
    private readonly Dictionary<(string, string, string, int), DictionaryEntry> _categoryEntries = new();
    private readonly HashSet<(string, string?, string?)> _allowed = [];
    private readonly Dictionary<(string, string), string> _canonicalNames = new();

    internal IReadOnlyList<ExtractionRule> ColorRules { get; private init; } = [];
    internal IReadOnlyList<ExtractionRule> OtherRules { get; private init; } = [];
    internal IReadOnlyList<(string Code, string Label, string Pattern)> ProductTypeRules { get; private init; } = [];
    internal IReadOnlySet<int> EntryCategoryIds { get; private init; } = new HashSet<int>();
    internal IReadOnlySet<int> InteriorCategoryIds { get; private init; } = new HashSet<int>();
    internal IReadOnlySet<int> CylinderCategoryIds { get; private init; } = new HashSet<int>();
    internal IReadOnlySet<int> SmartCategoryIds { get; private init; } = new HashSet<int>();
    internal IReadOnlySet<int> KnownCategoryMismatchIds { get; private init; } = new HashSet<int>();
    internal IReadOnlyDictionary<string, decimal> UnitFactorsToMm { get; private init; } = new Dictionary<string, decimal>();
    internal string DefaultDimensionUnit { get; private init; } = "mm";
    internal bool AllowKnownComponentCombinations { get; private init; }

    private RecognitionDictionary() { }

    /// <summary>
    /// Reads the dictionary file. Category groups (entrance and interior doors, cylinders, SMART) are completed from
    /// the category paths the same way as the Python recognizer.
    /// </summary>
    public static RecognitionDictionary Read(Stream stream, CategoryPaths categories,
        IEnumerable<AttributeCodeDecision>? decisions = null)
    {
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        if (Text(root, "approval_status") != ApprovalStatus)
            throw new InvalidDataException("The dictionary has no stage 4 approval status.");

        var config = root.GetProperty("parser_configuration");
        var rules = root.GetProperty("extraction_rules").EnumerateArray().Select(ReadRule).ToList();
        var entry = Ids(config, "entry_category_ids");
        var interior = Ids(config, "interior_category_ids");
        var cylinder = Ids(config, "cylinder_category_ids");
        var smart = Ids(config, "smart_category_ids");
        foreach (var (id, path) in categories.Paths)
        {
            if (path.Contains("Вхідні двері", StringComparison.Ordinal))
                entry.Add(id);
            if (path.Contains("Міжкімнатні двері", StringComparison.Ordinal) || path.EndsWith(" > Виставка", StringComparison.Ordinal))
                interior.Add(id);
            if (path.Contains("Циліндри", StringComparison.Ordinal))
            {
                if (path.Contains("SMART", StringComparison.Ordinal))
                    smart.Add(id);
                else
                    cylinder.Add(id);
            }
        }

        var dictionary = new RecognitionDictionary
        {
            ColorRules = rules.Where(rule => rule.Attribute == "color").ToList(),
            OtherRules = rules.Where(rule => rule.Attribute != "color").ToList(),
            ProductTypeRules = config.GetProperty("product_type_rules").EnumerateArray()
                .Select(rule => (rule[0].GetString()!, rule[1].GetString()!, rule[2].GetString()!)).ToList(),
            EntryCategoryIds = entry,
            InteriorCategoryIds = interior,
            CylinderCategoryIds = cylinder,
            SmartCategoryIds = smart,
            KnownCategoryMismatchIds = Ids(config, "known_category_mismatch_ids"),
            UnitFactorsToMm = config.GetProperty("unit_factors_to_mm").EnumerateObject()
                .ToDictionary(unit => unit.Name, unit => unit.Value.GetDecimal(), StringComparer.Ordinal),
            DefaultDimensionUnit = Text(config, "default_dimension_unit") ?? "mm",
            AllowKnownComponentCombinations = config.TryGetProperty("allow_known_component_combinations", out var allow)
                                              && allow.ValueKind == JsonValueKind.True
        };

        foreach (var value in root.GetProperty("canonical_values").EnumerateArray())
            dictionary._canonicalNames.TryAdd((Text(value, "attribute")!, Text(value, "code")!), Text(value, "name_uk")!);

        foreach (var item in root.GetProperty("entries").EnumerateArray())
            dictionary.AddEntry(ReadEntry(item));

        foreach (var decision in decisions ?? [])
            dictionary.Decide(decision);

        return dictionary;
    }

    /// <summary>
    /// The known spelling for what was found, if any: the owner's decision for the product's category first, then exact
    /// text, then folded (case, і/i, spaces).
    /// </summary>
    internal DictionaryEntry? Find(string attribute, string scope, string rawValue, int? categoryId = null)
        => (categoryId is { } category
               ? _categoryEntries.GetValueOrDefault((attribute, scope, Folded(rawValue), category))
               : null)
           ?? _entries.GetValueOrDefault((attribute, scope, rawValue))
           ?? _foldedEntries.GetValueOrDefault((attribute, scope, Folded(rawValue)));

    internal bool IsAllowed(string attribute, string? code, string? nameUk) => _allowed.Contains((attribute, code, nameUk));

    /// <summary>Python <c>folded</c>: case-folded, Latin i as Ukrainian і, single spaces.</summary>
    internal static string Folded(string text)
        => PythonRegex.Replace(@"\s+", text.ToLowerInvariant().Replace('i', 'і'), " ").Trim();

    private void AddEntry(DictionaryEntry entry)
    {
        var key = (entry.Attribute, entry.Scope, entry.RawValue);
        if (_entries.TryGetValue(key, out var old)
            && (old.Status, old.NormalizedCode, old.NormalizedValueUk) != (entry.Status, entry.NormalizedCode, entry.NormalizedValueUk))
        {
            throw new InvalidDataException($"Conflicting dictionary mapping: {key}.");
        }

        _entries[key] = entry;
        _foldedEntries[(entry.Attribute, entry.Scope, Folded(entry.RawValue))] = entry;
        if (entry.Status == RecognitionStatus.Approved)
        {
            if (entry.NormalizedCode is null || entry.NormalizedValueUk is null)
                throw new InvalidDataException($"Approved spelling without a value: {key}.");
            _allowed.Add((entry.Attribute, entry.NormalizedCode, entry.NormalizedValueUk));
        }
    }

    /// <summary>
    /// The owner's decision replaces the dictionary decoding of that spelling (or adds it): a value, confirmed by a
    /// person, or no value, so the recognizer ignores the spelling.
    /// </summary>
    private void Decide(AttributeCodeDecision decision)
    {
        var current = Find(decision.Attribute, decision.Scope, decision.RawValue);
        var name = decision.ValueCode is null
            ? null
            : _canonicalNames.GetValueOrDefault((decision.Attribute, decision.ValueCode)) ?? decision.ValueNameUk;
        var decided = new DictionaryEntry(
            decision.Attribute, decision.Scope, current?.RawValue ?? decision.RawValue,
            RecognitionStatus.Approved, decision.ValueCode, name,
            current?.Note, current?.SourceRefs ?? ["owner"], null, current?.EligibleNamePattern)
        {
            IsIgnored = decision.ValueCode is null
        };

        if (decision.CategoryId is { } categoryId)
            _categoryEntries[(decided.Attribute, decided.Scope, Folded(decided.RawValue), categoryId)] = decided;
        else
        {
            _entries[(decided.Attribute, decided.Scope, decided.RawValue)] = decided;
            _foldedEntries[(decided.Attribute, decided.Scope, Folded(decided.RawValue))] = decided;
        }

        if (decided.NormalizedCode is not null && name is not null)
            _allowed.Add((decided.Attribute, decided.NormalizedCode, name));
    }

    private static ExtractionRule ReadRule(JsonElement rule)
        => new(Text(rule, "attribute")!, Text(rule, "normalized_code"), Text(rule, "normalized_value_uk"),
            Text(rule, "pattern")!, Text(rule, "status")!, Text(rule, "note"), Strings(rule, "source_refs"),
            Text(rule, "candidate"), Text(rule, "kind"), Text(rule, "eligible_name_pattern"));

    private static DictionaryEntry ReadEntry(JsonElement entry)
        => new(Text(entry, "attribute")!, Text(entry, "scope")!, Text(entry, "raw_value")!, Text(entry, "status")!,
            Text(entry, "normalized_code"), Text(entry, "normalized_value_uk"), Text(entry, "note"),
            Strings(entry, "source_refs"), Text(entry, "selected_by"), Text(entry, "eligible_name_pattern"));

    private static HashSet<int> Ids(JsonElement config, string name)
        => config.TryGetProperty(name, out var ids) ? ids.EnumerateArray().Select(id => id.GetInt32()).ToHashSet() : [];

    private static IReadOnlyList<string> Strings(JsonElement element, string name)
        => element.TryGetProperty(name, out var values) && values.ValueKind == JsonValueKind.Array
            ? values.EnumerateArray().Select(value => value.GetString()!).ToList()
            : [];

    private static string? Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}
