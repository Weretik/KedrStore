namespace Catalog.Application.Features.Groups.Shared;

/// <summary>Validation errors of the group commands (field, message, code).</summary>
internal static class ProductGroupValidation
{
    public static Result Invalid(string field, string message, string code)
        => Result.Invalid(new ValidationError(field, message, $"Catalog.ProductGroup.{code}", ValidationSeverity.Error));
}
