using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.UploadAttributeSwatch;

/// <summary>An uploaded swatch file as the API received it.</summary>
public sealed record SwatchUpload(string FileName, long Length, Func<Stream> OpenReadStream);

/// <summary>
/// Puts a swatch picture into the media library before the value is saved; the answer's ID goes into the value's
/// <c>swatchMediaFileId</c>. Nothing about a value changes until it is saved.
/// </summary>
public sealed record UploadAttributeSwatchCommand(SwatchUpload File) : ICommand<Result<AdminAttributeSwatchUploadDto>>;
