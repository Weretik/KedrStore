# ADR 0035: Use IFileStorageService

## Date
2025-06-17

## Status
Accepted

## Context
Managing files is a common requirement in modern applications, whether for storing user uploads, generating reports, or handling media assets. `IFileStorageService` provides a clean abstraction for interacting with file storage systems, enabling flexibility and scalability by supporting various storage providers such as local storage, cloud storage (e.g., AWS S3, Azure Blob Storage), and others.

## Decision
We decided to use `IFileStorageService` in the project to:

1. Provide a centralized and consistent way to interact with file storage systems.
2. Decouple the application layer from specific storage implementations.
3. Enable flexibility to switch between storage providers without affecting the application logic.
4. Align with best practices for clean and maintainable architecture.

## Consequences
### Positive
1. Improves maintainability by centralizing file storage logic.
2. Decouples the application layer from specific storage implementations.
3. Simplifies testing by allowing mock implementations of `IFileStorageService`.
4. Enhances scalability by supporting various storage providers.

### Negative
1. Adds complexity by introducing an additional abstraction layer.
2. Requires careful implementation to ensure security and consistency.

## Example
`IFileStorageService` is implemented as follows:

**IFileStorageService.cs**:
```csharp
public interface IFileStorageService
{
    FileInfo SaveFile(string containerName, string fileName, byte[] content, string contentType = null, Dictionary<string, string> metadata = null);
    Task<FileInfo> SaveFileAsync(string containerName, string fileName, byte[] content, string contentType = null, Dictionary<string, string> metadata = null, CancellationToken cancellationToken = default);
    byte[] GetFile(string containerName, string fileName);
    Task<byte[]> GetFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    bool DeleteFile(string containerName, string fileName);
    Task<bool> DeleteFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
}
```

**Usage in Application Layer**:
```csharp
public class ReportService
{
    private readonly IFileStorageService _fileStorageService;

    public ReportService(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<FileInfo> GenerateReportAsync(string reportName, byte[] reportContent, CancellationToken cancellationToken)
    {
        return await _fileStorageService.SaveFileAsync("reports", reportName, reportContent, "application/pdf", cancellationToken: cancellationToken);
    }
}
```
