namespace Application.Extensions;

public static class MudBlazorValidationExtensions
{
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue<T>(
        this IValidator<T> validator,
        T model) where T : class
    {
        return async (_, propertyName) =>
        {
            var context = ValidationContext<T>.CreateWithOptions(model,
                x => x.IncludeProperties(propertyName));

            ValidationResult result = await validator.ValidateAsync(context);

            return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
