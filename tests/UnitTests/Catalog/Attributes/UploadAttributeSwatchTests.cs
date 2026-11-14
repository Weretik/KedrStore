using Ardalis.Result;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Features.Attributes.Commands.UploadAttributeSwatch;

namespace UnitTests.Catalog.Attributes;

/// <summary>A color or decor picture goes to the media library before the value is saved.</summary>
public sealed class UploadAttributeSwatchTests
{
    [Fact]
    public async Task Handle_StoresBytesUnderTheFileName_AndReturnsLibraryId()
    {
        var storage = new FakeStorage(Result.Success(new StoredSwatchImage(42, "https://cdn/swatches/a.png")));
        var bytes = new byte[] { 1, 2, 3 };

        var result = await new UploadAttributeSwatchHandler(storage).Handle(
            new UploadAttributeSwatchCommand(new SwatchUpload("C:\\photos\\satin.png", bytes.Length,
                () => new MemoryStream(bytes))), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value.MediaFileId);
        Assert.Equal("https://cdn/swatches/a.png", result.Value.Url);
        Assert.Equal(bytes, storage.Data);
        Assert.EndsWith("satin.png", storage.FileName);
    }

    [Fact]
    public async Task Handle_RejectsAStreamLargerThanTheLimit_WithoutStoring()
    {
        var storage = new FakeStorage(Result.Success(new StoredSwatchImage(1, "u")));
        var big = new byte[UploadAttributeSwatchValidator.MaxUploadBytes + 1];

        var result = await new UploadAttributeSwatchHandler(storage).Handle(
            new UploadAttributeSwatchCommand(new SwatchUpload("big.jpg", 10, () => new MemoryStream(big))),
            CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(storage.Data);
    }

    [Fact]
    public async Task Handle_PassesStorageErrorsThrough()
    {
        var storage = new FakeStorage(Result<StoredSwatchImage>.Unavailable("Media storage is not configured."));

        var result = await new UploadAttributeSwatchHandler(storage).Handle(
            new UploadAttributeSwatchCommand(new SwatchUpload("a.webp", 1, () => new MemoryStream([7]))),
            CancellationToken.None);

        Assert.Equal(ResultStatus.Unavailable, result.Status);
    }

    [Fact]
    public void Validator_RejectsEmptyAndOversizedFiles()
    {
        var validator = new UploadAttributeSwatchValidator();

        Assert.False(validator.Validate(new UploadAttributeSwatchCommand(
            new SwatchUpload("a.png", 0, () => Stream.Null))).IsValid);
        Assert.False(validator.Validate(new UploadAttributeSwatchCommand(new SwatchUpload("a.png",
            UploadAttributeSwatchValidator.MaxUploadBytes + 1, () => Stream.Null))).IsValid);
        Assert.True(validator.Validate(new UploadAttributeSwatchCommand(
            new SwatchUpload("a.png", 1024, () => Stream.Null))).IsValid);
    }

    private sealed class FakeStorage(Result<StoredSwatchImage> answer) : IAttributeSwatchStorage
    {
        public string? FileName { get; private set; }
        public byte[]? Data { get; private set; }

        public Task<Result<StoredSwatchImage>> UploadAsync(string fileName, byte[] data,
            CancellationToken cancellationToken)
        {
            FileName = fileName;
            Data = data;
            return Task.FromResult(answer);
        }
    }
}
