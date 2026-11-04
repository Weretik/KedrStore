namespace Media.Contracts.Files;

/// <summary>
/// Implemented by modules that link to library files. Media calls every registered listener after its own save,
/// so a failing listener never undoes the change; the module repairs its copy on its next sync.
/// </summary>
public interface IMediaFileChangeListener
{
    Task OnFileReplacedAsync(long fileId, string url, CancellationToken cancellationToken);

    Task OnFileDeletedAsync(long fileId, CancellationToken cancellationToken);
}
