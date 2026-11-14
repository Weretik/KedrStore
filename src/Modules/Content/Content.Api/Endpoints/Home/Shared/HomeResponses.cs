using Content.Application.Features.Home.Shared;
namespace Content.Api.Endpoints.Home.Shared;

public sealed record PublicHomeFaqItemResponse(string Question, string Answer);

public sealed record PublicHomeBrandResponse(string Name, string LogoUrl, int? Width, int? Height);

/// <summary>Home page FAQ and brands in one language, in site order.</summary>
public sealed record PublicHomeBlocksResponse(
    IReadOnlyList<PublicHomeFaqItemResponse> Faq,
    IReadOnlyList<PublicHomeBrandResponse> Brands);

public sealed record AdminHomeFaqItemResponse(
    long Id,
    int Position,
    string QuestionUk,
    string QuestionRu,
    string AnswerUk,
    string AnswerRu,
    DateTimeOffset UpdatedAt);

/// <summary>An admin brand; <c>logoMissing</c> marks a logo whose library file was deleted.</summary>
public sealed record AdminHomeBrandResponse(
    long Id,
    int Position,
    string Name,
    long LogoMediaFileId,
    string? LogoUrl,
    string? LogoFileName,
    bool LogoMissing,
    DateTimeOffset UpdatedAt);

public sealed record AdminHomeBlocksResponse(
    IReadOnlyList<AdminHomeFaqItemResponse> Faq,
    IReadOnlyList<AdminHomeBrandResponse> Brands);

public sealed record HomeFaqItemRequest(string? QuestionUk, string? QuestionRu, string? AnswerUk, string? AnswerRu);

public sealed record HomeBrandRequest(string? Name, long LogoMediaFileId);

public sealed record ReorderHomeRequest(IReadOnlyList<long>? Ids);

internal static class HomeResponseMapper
{
    public static PublicHomeBlocksResponse ToResponse(PublicHomeBlocks blocks) =>
        new(blocks.Faq.Select(item => new PublicHomeFaqItemResponse(item.Question, item.Answer)).ToList(),
            blocks.Brands.Select(brand => new PublicHomeBrandResponse(brand.Name, brand.LogoUrl, brand.Width,
                brand.Height)).ToList());

    public static AdminHomeBlocksResponse ToResponse(AdminHomeBlocks blocks) =>
        new(blocks.Faq.Select(item => new AdminHomeFaqItemResponse(item.Id, item.Position, item.QuestionUk,
                item.QuestionRu, item.AnswerUk, item.AnswerRu, item.UpdatedAt)).ToList(),
            blocks.Brands.Select(brand => new AdminHomeBrandResponse(brand.Id, brand.Position, brand.Name,
                brand.LogoMediaFileId, brand.LogoUrl, brand.LogoFileName, brand.LogoMissing, brand.UpdatedAt)).ToList());

    public static HomeFaqInput ToInput(HomeFaqItemRequest request) =>
        new(request.QuestionUk, request.QuestionRu, request.AnswerUk, request.AnswerRu);

    public static HomeBrandInput ToInput(HomeBrandRequest request) => new(request.Name, request.LogoMediaFileId);
}
