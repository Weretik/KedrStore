namespace Catalog.Application.Attributes.Recognition;

/// <summary>One part of a component color: as written, its decoding, and whether it is decoded.</summary>
internal sealed record ColorComponent(string RawValue, string? NormalizedCode, string? NormalizedValueUk, string Status);

/// <summary>
/// Something found in a product name: an attribute value as written (with its position) and its decoding.
/// <see cref="Status"/> is <see cref="RecognitionStatus.Approved"/> or <see cref="RecognitionStatus.Unresolved"/>.
/// </summary>
internal sealed class NameObservation
{
    public required string Attribute { get; init; }
    public required string Scope { get; set; }
    public required string RawValue { get; init; }
    public required int Start { get; init; }
    public required int End { get; init; }
    public string? NormalizedCode { get; set; }
    public string? NormalizedValueUk { get; set; }
    public required string Status { get; set; }
    public string? Note { get; set; }
    public IReadOnlyList<string>? SourceRefs { get; set; }
    public string? Candidate { get; set; }
    public IReadOnlyList<ColorComponent>? Components { get; set; }
    public string? SelectedBy { get; set; }

    public bool IsApproved => Status == RecognitionStatus.Approved;

    public ColorComponent AsComponent() => new(RawValue, NormalizedCode, NormalizedValueUk, Status);
}

internal static class RecognitionStatus
{
    public const string Approved = "approved";
    public const string Unresolved = "unresolved";
}
