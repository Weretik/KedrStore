namespace Catalog.Application.Attributes.Recognition;

/// <summary>The recognizer dictionary built into the assembly from <c>docs/catalog-attributes/dictionaries.json</c>.</summary>
public static class RecognitionDictionaryFile
{
    private const string ResourceName = "Catalog.Application.Attributes.Recognition.dictionaries.json";

    public static Stream Open()
        => typeof(RecognitionDictionaryFile).Assembly.GetManifestResourceStream(ResourceName)
           ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
}
