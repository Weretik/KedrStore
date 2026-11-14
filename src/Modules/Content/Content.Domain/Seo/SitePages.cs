namespace Content.Domain.Seo;

/// <summary>
/// The site pages that exist once (no slug) and whose search title and description an editor may set. The order is
/// the site menu order the admin list follows. Search, checkout, and pages with their own texts (categories,
/// products, articles) are not here.
/// </summary>
public static class SitePages
{
    public const string Home = "Home";
    public const string Catalog = "Catalog";
    public const string Wholesale = "Wholesale";
    public const string DeliveryPayment = "DeliveryPayment";
    public const string ReturnsExchanges = "ReturnsExchanges";
    public const string Contacts = "Contacts";
    public const string Gallery = "Gallery";
    public const string Articles = "Articles";
    public const string Requisites = "Requisites";
    public const string PublicOffer = "PublicOffer";
    public const string PrivacyPolicy = "PrivacyPolicy";

    public const int MaxLength = 32;

    public static readonly IReadOnlyList<string> All =
    [
        Home, Catalog, Wholesale, DeliveryPayment, ReturnsExchanges, Contacts, Gallery, Articles, Requisites,
        PublicOffer, PrivacyPolicy,
    ];

    public static bool IsKnown(string? page) => page is not null && All.Contains(page);
}
