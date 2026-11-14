using Content.Application.Features.Wholesale.Shared;

namespace Content.Api.Endpoints.Wholesale;

public sealed record WholesaleStepPayload(string Title, string Text);

public sealed record WholesaleFaqPayload(string Question, string Answer);

/// <summary>
/// One language of the page «Співпраця», block by block from the top: hero with its contact card, assortment,
/// company, three order steps, questions, closing call. Plain text; buttons and links stay in the site.
/// </summary>
public sealed record WholesaleTextPayload(
    string Language,
    string HeroTitle,
    string HeroLead,
    string ContactCardTitle,
    string ContactCardNote,
    string AssortmentTitle,
    string AssortmentIntro,
    string CompanyTitle,
    string CompanyText,
    string SelectionTitle,
    string SelectionText,
    string WarehouseTitle,
    string WarehouseText,
    string StepsTitle,
    IReadOnlyList<WholesaleStepPayload> Steps,
    string FaqTitle,
    IReadOnlyList<WholesaleFaqPayload> Faq,
    string ClosingTitle);

public sealed record PublicWholesalePageResponse(WholesaleTextPayload Text, DateTimeOffset UpdatedAt);

public sealed record AdminWholesalePageResponse(
    IReadOnlyList<WholesaleTextPayload> Texts,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

/// <summary>Both languages are replaced at once.</summary>
public sealed record SaveWholesalePageRequest(IReadOnlyList<WholesaleTextPayload> Texts);

internal static class WholesaleResponseMapper
{
    public static PublicWholesalePageResponse ToResponse(PublicWholesalePage page)
        => new(ToPayload(page.Text), page.UpdatedAt);

    public static AdminWholesalePageResponse ToResponse(AdminWholesalePage page)
        => new(page.Texts.Select(ToPayload).ToList(), page.UpdatedAt, page.UpdatedBy);

    public static IReadOnlyList<WholesaleTextDto> ToInput(SaveWholesalePageRequest request)
        => (request.Texts ?? []).Select(text => text is null ? null! : new WholesaleTextDto(
            text.Language, text.HeroTitle, text.HeroLead, text.ContactCardTitle, text.ContactCardNote,
            text.AssortmentTitle, text.AssortmentIntro, text.CompanyTitle, text.CompanyText, text.SelectionTitle,
            text.SelectionText, text.WarehouseTitle, text.WarehouseText, text.StepsTitle,
            (text.Steps ?? []).Select(step => new WholesaleStepText(step.Title, step.Text)).ToList(),
            text.FaqTitle,
            (text.Faq ?? []).Select(item => new WholesaleFaqText(item.Question, item.Answer)).ToList(),
            text.ClosingTitle)).ToList();

    private static WholesaleTextPayload ToPayload(WholesaleTextDto text) => new(
        text.Language, text.HeroTitle, text.HeroLead, text.ContactCardTitle, text.ContactCardNote,
        text.AssortmentTitle, text.AssortmentIntro, text.CompanyTitle, text.CompanyText, text.SelectionTitle,
        text.SelectionText, text.WarehouseTitle, text.WarehouseText, text.StepsTitle,
        text.Steps.Select(step => new WholesaleStepPayload(step.Title, step.Text)).ToList(),
        text.FaqTitle, text.Faq.Select(item => new WholesaleFaqPayload(item.Question, item.Answer)).ToList(),
        text.ClosingTitle);
}
