namespace Identity.Infrastructure.Options;

public sealed class ImportedCounterpartyIdentityOptions
{
    public const string SectionName = "Identity:ImportedCounterparties";

    public bool LockoutEnabled { get; init; }
    public bool EmailConfirmed { get; init; } = true;
}
