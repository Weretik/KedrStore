using Media.Contracts.Files;
using Microsoft.Extensions.Logging;

namespace Media.Application.Features.Files.Common;

/// <summary>
/// Tells the modules that link to files about a replace or delete, after the library has saved it. A listener
/// failure is logged and does not fail the request: the change is already on the CDN and in the library.
/// </summary>
public sealed class MediaFileChangeNotifier(
    IEnumerable<IMediaFileChangeListener> listeners,
    ILogger<MediaFileChangeNotifier> logger)
{
    public Task ReplacedAsync(long fileId, string url, CancellationToken cancellationToken)
        => NotifyAsync(fileId, listener => listener.OnFileReplacedAsync(fileId, url, cancellationToken));

    public Task DeletedAsync(long fileId, CancellationToken cancellationToken)
        => NotifyAsync(fileId, listener => listener.OnFileDeletedAsync(fileId, cancellationToken));

    private async Task NotifyAsync(long fileId, Func<IMediaFileChangeListener, Task> notify)
    {
        foreach (var listener in listeners)
        {
            try
            {
                await notify(listener);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Listener {Listener} failed for media file {MediaFileId}",
                    listener.GetType().Name, fileId);
            }
        }
    }
}
