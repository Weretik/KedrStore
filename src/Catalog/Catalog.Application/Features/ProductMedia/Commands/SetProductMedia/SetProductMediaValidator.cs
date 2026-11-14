namespace Catalog.Application.Features.ProductMedia.Commands.SetProductMedia;

/// <summary>R-30, R-31: up to 20 photos, positive distinct IDs, a scheme that is not one of the photos.</summary>
public sealed class SetProductMediaValidator : AbstractValidator<SetProductMediaCommand>
{
    public SetProductMediaValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();

        RuleFor(command => command.Request.PhotoIds)
            .NotNull()
            .Must(ids => ids.Count <= Product.MaximumPhotos)
            .WithMessage($"A product can have at most {Product.MaximumPhotos} photos.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("A file can be added to the photos only once.")
            .When(command => command.Request is not null)
            .OverridePropertyName("photoIds");

        RuleForEach(command => command.Request.PhotoIds)
            .GreaterThan(0)
            .When(command => command.Request?.PhotoIds is not null)
            .OverridePropertyName("photoIds");

        RuleFor(command => command.Request.SchemeId)
            .GreaterThan(0)
            .Must((command, schemeId) => command.Request.PhotoIds?.Contains(schemeId!.Value) != true)
            .WithMessage("The scheme cannot also be a photo.")
            .When(command => command.Request?.SchemeId is not null)
            .OverridePropertyName("schemeId");
    }
}
