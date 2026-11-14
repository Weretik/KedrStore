namespace Catalog.Application.Features.Attributes.Commands.UploadAttributeSwatch;

public sealed class UploadAttributeSwatchValidator : AbstractValidator<UploadAttributeSwatchCommand>
{
    /// <summary>A swatch is a small circle on the site; a few megabytes is plenty for a photo of a finish.</summary>
    public const long MaxUploadBytes = 5 * 1024 * 1024;

    public UploadAttributeSwatchValidator()
    {
        RuleFor(x => x.File).NotNull().OverridePropertyName("file");
        RuleFor(x => x.File.Length)
            .GreaterThan(0).LessThanOrEqualTo(MaxUploadBytes)
            .When(x => x.File is not null)
            .WithMessage("The picture must be between 1 byte and 5 MB.")
            .OverridePropertyName("file");
    }
}
