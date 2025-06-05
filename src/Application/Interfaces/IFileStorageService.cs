

namespace Application.Interfaces
{
    /// <summary>
    /// Информация о файле в хранилище
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Путь к файлу в хранилище
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Размер файла в байтах
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// MIME-тип файла
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Дата создания файла
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата последнего изменения файла
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// URL для доступа к файлу (если доступен)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Метаданные файла
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Сервис для работы с файловым хранилищем
    /// </summary>
    /// <remarks>
    /// Этот интерфейс предоставляет абстракцию для работы с файлами,
    /// позволяя использовать различные реализации хранилища (локальное, облачное, S3, Azure и т.д.).
    /// </remarks>
    public interface IFileStorageService
    {
        /// <summary>
        /// Сохраняет файл в хранилище
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="content">Содержимое файла</param>
        /// <param name="contentType">MIME-тип содержимого</param>
        /// <param name="metadata">Метаданные файла</param>
        /// <returns>Информация о сохраненном файле</returns>
        FileInfo SaveFile(string containerName, string fileName, byte[] content, string contentType = null, Dictionary<string, string> metadata = null);

        /// <summary>
        /// Асинхронно сохраняет файл в хранилище
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="content">Содержимое файла</param>
        /// <param name="contentType">MIME-тип содержимого</param>
        /// <param name="metadata">Метаданные файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о сохраненном файле</returns>
        Task<FileInfo> SaveFileAsync(string containerName, string fileName, byte[] content, string contentType = null, Dictionary<string, string> metadata = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохраняет файл в хранилище из потока
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="stream">Поток с содержимым файла</param>
        /// <param name="contentType">MIME-тип содержимого</param>
        /// <param name="metadata">Метаданные файла</param>
        /// <returns>Информация о сохраненном файле</returns>
        FileInfo SaveFile(string containerName, string fileName, Stream stream, string contentType = null, Dictionary<string, string> metadata = null);

        /// <summary>
        /// Асинхронно сохраняет файл в хранилище из потока
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="stream">Поток с содержимым файла</param>
        /// <param name="contentType">MIME-тип содержимого</param>
        /// <param name="metadata">Метаданные файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о сохраненном файле</returns>
        Task<FileInfo> SaveFileAsync(string containerName, string fileName, Stream stream, string contentType = null, Dictionary<string, string> metadata = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает файл из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Содержимое файла</returns>
        byte[] GetFile(string containerName, string fileName);

        /// <summary>
        /// Асинхронно получает файл из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Содержимое файла</returns>
        Task<byte[]> GetFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает поток для чтения файла из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Поток для чтения файла</returns>
        Stream GetFileStream(string containerName, string fileName);

        /// <summary>
        /// Асинхронно получает поток для чтения файла из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Поток для чтения файла</returns>
        Task<Stream> GetFileStreamAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает информацию о файле
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Информация о файле</returns>
        FileInfo GetFileInfo(string containerName, string fileName);

        /// <summary>
        /// Асинхронно получает информацию о файле
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о файле</returns>
        Task<FileInfo> GetFileInfoAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет файл из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>true, если файл успешно удален, иначе false</returns>
        bool DeleteFile(string containerName, string fileName);

        /// <summary>
        /// Асинхронно удаляет файл из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true, если файл успешно удален, иначе false</returns>
        Task<bool> DeleteFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет существование файла в хранилище
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>true, если файл существует, иначе false</returns>
        bool FileExists(string containerName, string fileName);

        /// <summary>
        /// Асинхронно проверяет существование файла в хранилище
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true, если файл существует, иначе false</returns>
        Task<bool> FileExistsAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает URL для доступа к файлу
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="expiry">Время истечения URL (для временных URL)</param>
        /// <returns>URL для доступа к файлу</returns>
        string GetFileUrl(string containerName, string fileName, TimeSpan? expiry = null);

        /// <summary>
        /// Асинхронно получает URL для доступа к файлу
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="expiry">Время истечения URL (для временных URL)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>URL для доступа к файлу</returns>
        Task<string> GetFileUrlAsync(string containerName, string fileName, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает список файлов в контейнере
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="prefix">Префикс для фильтрации файлов</param>
        /// <returns>Список информации о файлах</returns>
        IEnumerable<FileInfo> ListFiles(string containerName, string prefix = null);

        /// <summary>
        /// Асинхронно получает список файлов в контейнере
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="prefix">Префикс для фильтрации файлов</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список информации о файлах</returns>
        Task<IEnumerable<FileInfo>> ListFilesAsync(string containerName, string prefix = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Создает контейнер в хранилище
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="isPublic">Флаг, указывающий, является ли контейнер публичным</param>
        /// <returns>true, если контейнер успешно создан, иначе false</returns>
        bool CreateContainer(string containerName, bool isPublic = false);

        /// <summary>
        /// Асинхронно создает контейнер в хранилище
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="isPublic">Флаг, указывающий, является ли контейнер публичным</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true, если контейнер успешно создан, иначе false</returns>
        Task<bool> CreateContainerAsync(string containerName, bool isPublic = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет контейнер из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <returns>true, если контейнер успешно удален, иначе false</returns>
        bool DeleteContainer(string containerName);

        /// <summary>
        /// Асинхронно удаляет контейнер из хранилища
        /// </summary>
        /// <param name="containerName">Имя контейнера или папки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true, если контейнер успешно удален, иначе false</returns>
        Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);
    }
}
