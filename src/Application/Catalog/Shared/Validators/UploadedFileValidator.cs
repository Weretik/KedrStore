namespace Application.Catalog.Shared.Validators;

public class UploadedFileValidator: AbstractValidator<UploadedFile>
{
    public UploadedFileValidator()
    {
        RuleFor(f => f.FileName)
            .NotEmpty().WithMessage("Ім'я файлу не вказано.")
            .MaximumLength(255).WithMessage("Ім'я файлу занадто довге.")
            .Must(NotContainInvalidChars)
            .WithMessage("Ім'я файлу містить неприпустимі символи.");

        RuleFor(f => f.ContentType)
            .Must(type => type is not null && AllowedContentTypes.Contains(type))
            .WithMessage(f => $"Тип файлу '{f.ContentType ?? "невідомий"}' не підтримується. " +
                              $"Дозволено тільки XML.");

        RuleFor(f => f.Length)
            .GreaterThan(0).WithMessage("Файл порожній.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"Розмір файлу перевищує допустимий ліміт {MaxFileSize / (1024 * 1024)} МБ.");

        RuleFor(f => f.OpenReadStream)
            .NotNull().WithMessage("Метод відкриття потоку відсутній.")
            .Must(BeCallable).WithMessage("Не вдалося відкрити потік читання файлу.");
    }
    private const long MaxFileSize = 50 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    {
        "application/xml",
        "text/xml"
    };

    private static bool BeCallable(Func<Stream> openStream)
    {
        try
        {
            using var stream = openStream.Invoke();
            return stream.CanRead;
        }
        catch
        {
            return false;
        }
    }

    private static bool NotContainInvalidChars(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return fileName.All(c => !invalidChars.Contains(c));
    }
}
