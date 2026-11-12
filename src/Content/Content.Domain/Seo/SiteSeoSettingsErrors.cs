namespace Content.Domain.Seo;

public static class SiteSeoSettingsErrors
{
    public static DomainError Invalid(string field, string? value) =>
        new($"Content.SiteSeoSettings.{field}.Invalid", $"{field} has an invalid value: {value}");
}
