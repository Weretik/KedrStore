namespace Catalog.Application.Features.SiteModels.Common;

/// <summary>Validation errors of the model commands (field, message, code).</summary>
internal static class SiteModelValidation
{
    public static Result Invalid(string field, string message, string code)
        => Result.Invalid(new ValidationError(field, message, $"Catalog.SiteModel.{code}", ValidationSeverity.Error));
}
