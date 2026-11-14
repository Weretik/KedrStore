namespace Content.Application.Features.Home.Shared;

public sealed record HomeFaqInput(string? QuestionUk, string? QuestionRu, string? AnswerUk, string? AnswerRu);

public sealed record HomeBrandInput(string? Name, long LogoMediaFileId);

public sealed record AdminHomeFaqItem(
    long Id,
    int Position,
    string QuestionUk,
    string QuestionRu,
    string AnswerUk,
    string AnswerRu,
    DateTimeOffset UpdatedAt);

/// <summary>An admin brand; <c>LogoMissing</c> marks a logo whose library file was deleted (the site hides it).</summary>
public sealed record AdminHomeBrand(
    long Id,
    int Position,
    string Name,
    long LogoMediaFileId,
    string? LogoUrl,
    string? LogoFileName,
    bool LogoMissing,
    DateTimeOffset UpdatedAt);

/// <summary>Both home page lists in site order; every admin change answers with them.</summary>
public sealed record AdminHomeBlocks(IReadOnlyList<AdminHomeFaqItem> Faq, IReadOnlyList<AdminHomeBrand> Brands);

public sealed record PublicHomeFaqItem(string Question, string Answer);

public sealed record PublicHomeBrand(string Name, string LogoUrl, int? Width, int? Height);

public sealed record PublicHomeBlocks(IReadOnlyList<PublicHomeFaqItem> Faq, IReadOnlyList<PublicHomeBrand> Brands);
