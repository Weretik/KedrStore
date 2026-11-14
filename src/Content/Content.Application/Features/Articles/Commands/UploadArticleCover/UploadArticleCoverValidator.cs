namespace Content.Application.Features.Articles.Commands.UploadArticleCover;

public sealed class UploadArticleCoverValidator : AbstractValidator<UploadArticleCoverCommand>
{
    /// <summary>Raw upload limit; the stored WebP is much smaller.</summary>
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    public UploadArticleCoverValidator()
    {
        RuleFor(x => x.File).NotNull().OverridePropertyName("file");
        RuleFor(x => x.File.Length)
            .GreaterThan(0).LessThanOrEqualTo(MaxUploadBytes)
            .When(x => x.File is not null)
            .WithMessage("The picture must be between 1 byte and 10 MB.")
            .OverridePropertyName("file");
    }
}
