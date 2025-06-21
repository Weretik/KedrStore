// Licensed to KedrStore Development Team under MIT License.

namespace Application.DTOs;

public sealed class FileDescriptor
{
    public string Name { get; set; }
    public string Path { get; set; }
    public long Size { get; set; }
    public string ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public string? Url { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
