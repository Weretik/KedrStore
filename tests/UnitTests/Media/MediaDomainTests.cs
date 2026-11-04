using BuildingBlocks.Domain.Exceptions;
using Media.Application.Features.Files.GetList;
using Media.Application.Features.Folders.Create;
using Media.Domain.Files;
using Media.Domain.Folders;

namespace UnitTests.Media;

public sealed class MediaDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Folder_NameIsTrimmed_AndMustNotBeEmptyOrTooLong()
    {
        Assert.Equal("Акції", MediaFolder.Create("  Акції  ", null, Now).Name);
        Assert.Throws<DomainException>(() => MediaFolder.Create("   ", null, Now));
        Assert.Throws<DomainException>(() => MediaFolder.Create(new string('a', MediaFolder.NameMaxLength + 1), null, Now));
    }

    [Fact]
    public void File_Replace_KeepsNameAndReturnsOldKey()
    {
        var file = MediaFile.Create(null, "731.jpg", new MediaFileContent("/products/731.jpg", "image/jpeg", 10, null, null), Now);
        Assert.Equal("products/731.jpg", file.StorageKey);

        var later = Now.AddDays(1);
        var old = file.Replace(new MediaFileContent("library/2026/11/a.webp", "image/webp", 20, 800, 600), later);

        Assert.Equal("products/731.jpg", old);
        Assert.Equal("library/2026/11/a.webp", file.StorageKey);
        Assert.Equal("731.jpg", file.FileName);
        Assert.Equal((800, 600), (file.Width!.Value, file.Height!.Value));
        Assert.Equal(later, file.UpdatedAt);
    }

    [Fact]
    public void File_RejectsEmptyKeyAndBadDimensions()
    {
        Assert.Throws<DomainException>(() =>
            MediaFile.Create(null, "a.jpg", new MediaFileContent(" ", "image/jpeg", 1, null, null), Now));
        Assert.Throws<DomainException>(() =>
            MediaFile.Create(null, "a.jpg", new MediaFileContent("a.jpg", "image/jpeg", 1, 0, 10), Now));
        Assert.Throws<DomainException>(() =>
            MediaFile.Create(null, " ", new MediaFileContent("a.jpg", "image/jpeg", 1, null, null), Now));
    }

    [Fact]
    public void Validators_RejectBadInput()
    {
        Assert.False(new CreateMediaFolderCommandValidator().Validate(new CreateMediaFolderCommand("", 0)).IsValid);
        Assert.True(new CreateMediaFolderCommandValidator().Validate(new CreateMediaFolderCommand("Схеми", null)).IsValid);

        var list = new GetMediaFilesQueryValidator();
        Assert.False(list.Validate(new GetMediaFilesQuery(5, true, null)).IsValid);
        Assert.False(list.Validate(new GetMediaFilesQuery(null, false, null, PageSize: 500)).IsValid);
        Assert.True(list.Validate(new GetMediaFilesQuery(null, true, "731")).IsValid);
    }
}
