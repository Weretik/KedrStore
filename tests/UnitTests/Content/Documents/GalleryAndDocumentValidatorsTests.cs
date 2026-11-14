using BuildingBlocks.Application.Validation;
using Content.Application.Features.Documents.Commands.CreateLegalDocumentDraft;
using Content.Application.Features.Documents.Shared;
using Content.Application.Features.Gallery.Commands.AddGalleryPhotos;
using Content.Application.Features.Gallery.Commands.AddGalleryVideo;
using Content.Application.Features.Gallery.Commands.ReorderGallery;
using Content.Application.Features.Gallery.Shared;
using Content.Domain.Gallery;
namespace UnitTests.Content.Documents;

/// <summary>Gallery and legal document input checked before the handlers run.</summary>
public sealed class GalleryAndDocumentValidatorsTests
{
    public GalleryAndDocumentValidatorsTests() => UkrainianValidationMessages.Use();

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", true)]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", true)]
    [InlineData("https://vimeo.com/123", false)]
    [InlineData("", false)]
    public void Video_AcceptsOnlyYoutubeLinks(string url, bool valid)
    {
        var result = new AddGalleryVideoValidator().Validate(new AddGalleryVideoCommand(url, null));

        Assert.Equal(valid, result.IsValid);
        if (!valid)
            Assert.Equal("Content.Gallery.Youtube.Invalid", Assert.Single(result.Errors).ErrorCode);
    }

    [Fact]
    public void Video_ChecksTextLimits()
    {
        var texts = new GalleryTextsInput(new string('т', GalleryItemTexts.TitleMaxLength + 1), null,
            null, new string('п', GalleryItemTexts.CaptionMaxLength + 1));

        var result = new AddGalleryVideoValidator().Validate(
            new AddGalleryVideoCommand("https://youtu.be/dQw4w9WgXcQ", texts));

        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Photos_LimitTheBatchAndRefuseBadIds()
    {
        var validator = new AddGalleryPhotosValidator();
        var tooMany = Enumerable.Range(1, AddGalleryPhotosValidator.MaxFiles + 1).Select(i => (long)i).ToList();

        Assert.False(validator.Validate(new AddGalleryPhotosCommand([])).IsValid);
        Assert.False(validator.Validate(new AddGalleryPhotosCommand([1, 0])).IsValid);
        Assert.Equal("За раз можна додати не більше 100 фото.",
            Assert.Single(validator.Validate(new AddGalleryPhotosCommand(tooMany)).Errors).ErrorMessage);
    }

    [Fact]
    public void Reorder_RefusesRepeatedCards()
        => Assert.Equal("Кожна картка має бути лише один раз.", Assert.Single(new ReorderGalleryValidator()
            .Validate(new ReorderGalleryCommand([1, 2, 1])).Errors).ErrorMessage);

    [Fact]
    public void LegalDocument_NeedsBothLanguagesOnce()
    {
        var validator = new CreateLegalDocumentDraftValidator();
        var uk = new LegalDocumentTextData("uk", "Умови", "<p>Текст</p>");
        var ru = new LegalDocumentTextData("ru", "Условия", "<p>Текст</p>");

        Assert.True(validator.Validate(new CreateLegalDocumentDraftCommand("offer", [uk, ru])).IsValid);
        Assert.True(validator.Validate(new CreateLegalDocumentDraftCommand("offer", null)).IsValid);
        Assert.Equal("Потрібні тексти українською та російською, по одному.", Assert.Single(validator
            .Validate(new CreateLegalDocumentDraftCommand("offer", [uk, uk])).Errors).ErrorMessage);
    }

    [Fact]
    public void LegalDocument_TextsNeedTitleAndBody()
    {
        var result = new CreateLegalDocumentDraftValidator().Validate(new CreateLegalDocumentDraftCommand("",
            [new LegalDocumentTextData("uk", "", "<p>Текст</p>"), new LegalDocumentTextData("ru", "Условия", "")]));

        Assert.Equal(3, result.Errors.Count);
    }
}
