namespace Application.Interfaces
{
    public interface IFileStorageService
{
    #region Upload

    /// <summary>Сохраняет файл по содержимому (byte[])</summary>
    Task<FileDescriptor> SaveFileAsync(
        string container,
        string fileName,
        byte[] content,
        string? contentType = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>Сохраняет файл из потока (Stream)</summary>
    Task<FileDescriptor> SaveFileAsync(
        string container,
        string fileName,
        Stream stream,
        string? contentType = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Download

    /// <summary>Возвращает содержимое файла в байтах</summary>
    Task<byte[]> GetFileAsync(
        string container,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Возвращает поток для чтения файла</summary>
    Task<Stream> GetFileStreamAsync(
        string container,
        string fileName,
        CancellationToken cancellationToken = default);

    #endregion

    #region Info

    /// <summary>Получает информацию о файле</summary>
    Task<FileDescriptor> GetFileInfoAsync(
        string container,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Проверяет существование файла</summary>
    Task<bool> FileExistsAsync(
        string container,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>Возвращает публичный URL (если доступен)</summary>
    Task<string> GetFileUrlAsync(
        string container,
        string fileName,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Delete

    /// <summary>Удаляет файл</summary>
    Task<bool> DeleteFileAsync(
        string container,
        string fileName,
        CancellationToken cancellationToken = default);

    #endregion

    #region Container (optional)

    /// <summary>Создаёт контейнер (папку / bucket)</summary>
    Task<bool> CreateContainerAsync(
        string container,
        bool isPublic = false,
        CancellationToken cancellationToken = default);

    /// <summary>Удаляет контейнер</summary>
    Task<bool> DeleteContainerAsync(
        string container,
        CancellationToken cancellationToken = default);

    /// <summary>Возвращает список всех файлов в контейнере</summary>
    Task<IEnumerable<FileDescriptor>> ListFilesAsync(
        string container,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    #endregion
}
}
