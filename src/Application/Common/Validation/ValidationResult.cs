namespace Application.Common.Validation
{
    public sealed record class ValidationResult(IReadOnlyList<ValidationError> Errors)
    {
        public bool IsValid => Errors.Count == 0;

        public static ValidationResult Success() => new([]);

        public static ValidationResult Failure(params ValidationError[] errors) => new(errors);
    }
}
