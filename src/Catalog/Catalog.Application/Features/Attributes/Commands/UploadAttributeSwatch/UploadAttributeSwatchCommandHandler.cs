using Catalog.Application.Abstractions.Media;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.UploadAttributeSwatch;

/// <summary>Stores the picture as it is (JPEG, PNG, or WebP) on the CDN under <c>swatches/</c>, folder «Кольори».</summary>
public sealed class UploadAttributeSwatchCommandHandler(IAttributeSwatchStorage storage)
    : ICommandHandler<UploadAttributeSwatchCommand, Result<AdminAttributeSwatchUploadDto>>
{
    public async ValueTask<Result<AdminAttributeSwatchUploadDto>> Handle(UploadAttributeSwatchCommand command,
        CancellationToken cancellationToken)
    {
        var data = await ReadLimitedAsync(command.File.OpenReadStream, cancellationToken);
        if (data is null)
            return Result<AdminAttributeSwatchUploadDto>.Invalid(
                new ValidationError("file", "The picture must be between 1 byte and 5 MB.", null, ValidationSeverity.Error));

        var name = Path.GetFileName(command.File.FileName);
        var stored = await storage.UploadAsync(string.IsNullOrWhiteSpace(name) ? "swatch" : name, data,
            cancellationToken);
        return stored.Map(image => new AdminAttributeSwatchUploadDto(image.MediaFileId, image.Url));
    }

    private static async Task<byte[]?> ReadLimitedAsync(Func<Stream> open, CancellationToken cancellationToken)
    {
        await using var content = open();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > UploadAttributeSwatchCommandValidator.MaxUploadBytes)
                return null;
            buffer.Write(chunk, 0, read);
        }

        return buffer.Length == 0 ? null : buffer.ToArray();
    }
}
