namespace Content.Application.Features.Gallery.Shared;

internal static class GalleryTexts
{
    public static GalleryItemTexts ToDomain(GalleryTextsInput? input) => input is null
        ? GalleryItemTexts.Empty
        : GalleryItemTexts.Create(input.TitleUk, input.TitleRu, input.CaptionUk, input.CaptionRu);
}
