namespace Content.Application.Features.Gallery.Common;

public sealed class GalleryTextsInputValidator : AbstractValidator<GalleryTextsInput>
{
    public GalleryTextsInputValidator()
    {
        RuleFor(x => x.TitleUk).MaximumLength(GalleryItemTexts.TitleMaxLength);
        RuleFor(x => x.TitleRu).MaximumLength(GalleryItemTexts.TitleMaxLength);
        RuleFor(x => x.CaptionUk).MaximumLength(GalleryItemTexts.CaptionMaxLength);
        RuleFor(x => x.CaptionRu).MaximumLength(GalleryItemTexts.CaptionMaxLength);
    }
}
