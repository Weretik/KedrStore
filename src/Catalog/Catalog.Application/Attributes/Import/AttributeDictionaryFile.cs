using System.Text.Json;

namespace Catalog.Application.Attributes.Import;

/// <summary>
/// Reads the attribute dictionary (<c>docs/catalog-attributes/dictionaries.json</c>, format 1.0): attribute labels,
/// canonical values, and the spellings (entries) the recognizer maps to them.
/// </summary>
public static class AttributeDictionaryFile
{
    private const string Approved = "approved";
    private const string Assistant = "assistant";

    public static AttributeDictionary Read(Stream stream)
    {
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;

        var labels = root.GetProperty("attribute_labels_uk").EnumerateObject()
            .Select(property => (Code: property.Name, Name: property.Value.GetString() ?? property.Name))
            .ToList();
        foreach (var code in AttributeImportCatalog.SizeNumberAttributes.Prepend(AttributeImportCatalog.Size))
        {
            if (labels.All(label => label.Code != code))
                labels.Add((code, code));
        }

        var attributes = labels
            .Select((label, index) => new AttributeDefinition(
                label.Code,
                AttributeImportCatalog.NameOf(label.Code, label.Name),
                AttributeImportCatalog.KindOf(label.Code),
                AttributeImportCatalog.UnitOf(label.Code),
                AttributeImportCatalog.SlugOf(label.Code) ?? string.Empty,
                AttributeImportCatalog.IsFilterable(label.Code),
                AttributeImportCatalog.IsVariantAxis(label.Code),
                AttributeImportCatalog.SortOrderOf(label.Code, index)))
            .ToList();

        var values = root.GetProperty("canonical_values").EnumerateArray()
            .Select((value, index) => new AttributeValueDefinition(
                Text(value, "attribute")!, Text(value, "code")!, Text(value, "name_uk")!, index))
            .ToList();

        var aliases = root.GetProperty("entries").EnumerateArray()
            .Select(entry =>
            {
                var approved = Text(entry, "status") == Approved;
                var pickedByAssistant = Text(entry, "selected_by") == Assistant;
                var confirmedByOwner = entry.TryGetProperty("user_confirmed", out var confirmed)
                                       && confirmed.ValueKind == JsonValueKind.True;
                return new AttributeAliasDefinition(
                    Text(entry, "attribute")!,
                    Text(entry, "scope") ?? "general",
                    Text(entry, "raw_value")!,
                    approved ? Text(entry, "normalized_code") : null,
                    !pickedByAssistant || confirmedByOwner,
                    Text(entry, "note"));
            })
            .ToList();

        return new AttributeDictionary(attributes, values, aliases);
    }

    private static string? Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
