using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Domain.Seo;

public static class SiteSeoSettingsErrors
{
    public static DomainError Invalid(string field, string? value) =>
        new($"Content.SiteSeoSettings.{field}.Invalid", $"Некоректне значення: {value}.", Field: field);
}
