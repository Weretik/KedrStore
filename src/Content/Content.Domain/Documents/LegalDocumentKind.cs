namespace Content.Domain.Documents;

/// <summary>The legal texts the site publishes; each has its own version history.</summary>
public enum LegalDocumentKind
{
    PublicOffer = 1,
    PrivacyPolicy = 2,
    ReturnsPolicy = 3,
}

/// <summary>Where a version is in its life: only one draft and one published version per kind.</summary>
public enum LegalDocumentStatus
{
    Draft = 1,
    Published = 2,
    Archived = 3,
}
