namespace Catalog.Application.Features.Attributes.Common;

/// <summary>Validation errors of the attribute commands (field, message, code).</summary>
internal static class AttributeValidation
{
    public static Result Invalid(string field, string message, string code)
        => Result.Invalid(new ValidationError(field, message, $"Catalog.Attribute.{code}", ValidationSeverity.Error));
}
