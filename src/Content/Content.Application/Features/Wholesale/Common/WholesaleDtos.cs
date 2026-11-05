namespace Content.Application.Features.Wholesale.Common;

public sealed record WholesaleStepText(string Title, string Text);

public sealed record WholesaleFaqText(string Question, string Answer);

/// <summary>One language of the page «Співпраця»; the admin input and the stored text share this shape.</summary>
public sealed record WholesaleTextDto(
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
    IReadOnlyList<WholesaleStepText> Steps,
    string FaqTitle,
    IReadOnlyList<WholesaleFaqText> Faq,
    string ClosingTitle);

public sealed record PublicWholesalePage(WholesaleTextDto Text, DateTimeOffset UpdatedAt);

public sealed record AdminWholesalePage(IReadOnlyList<WholesaleTextDto> Texts, DateTimeOffset UpdatedAt, string? UpdatedBy);

internal static class WholesaleMapper
{
    public static WholesaleTextDto ToDto(string language, WholesalePageText text) => new(
        language, text.HeroTitle, text.HeroLead, text.ContactCardTitle, text.ContactCardNote, text.AssortmentTitle,
        text.AssortmentIntro, text.CompanyTitle, text.CompanyText, text.SelectionTitle, text.SelectionText,
        text.WarehouseTitle, text.WarehouseText, text.StepsTitle,
        text.Steps.Select(step => new WholesaleStepText(step.Title, step.Text)).ToList(),
        text.FaqTitle, text.Faq.Select(item => new WholesaleFaqText(item.Question, item.Answer)).ToList(),
        text.ClosingTitle);

    public static AdminWholesalePage ToAdmin(WholesalePage page) => new(
        WholesalePage.Languages.Select(language => ToDto(language, page.Text(language))).ToList(),
        page.UpdatedAt, page.UpdatedBy);

    public static WholesalePageText ToText(WholesaleTextDto input) => WholesalePageText.Create(new(
        input.HeroTitle, input.HeroLead, input.ContactCardTitle, input.ContactCardNote, input.AssortmentTitle,
        input.AssortmentIntro, input.CompanyTitle, input.CompanyText, input.SelectionTitle, input.SelectionText,
        input.WarehouseTitle, input.WarehouseText, input.StepsTitle,
        input.Steps.Select(step => (step.Title, step.Text)).ToList(),
        input.FaqTitle, input.Faq.Select(item => (item.Question, item.Answer)).ToList(),
        input.ClosingTitle));
}
