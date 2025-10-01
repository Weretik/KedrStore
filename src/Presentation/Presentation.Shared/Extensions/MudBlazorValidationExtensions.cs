namespace Presentation.Shared.Extensions;

public static class MudBlazorValidationExtensions
{
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue<T>(this IValidator<T> validator, T model)
        where T : class
    {
        return async (_, propertyName) =>
        {
            ValidationResult result = await validator.ValidateAsync(ValidationContext<T>.CreateWithOptions(model,
                x => x.IncludeProperties(propertyName)));

            return result.IsValid ? [] : result.Errors.Select(failure => failure.ErrorMessage);
        };
    }
}
