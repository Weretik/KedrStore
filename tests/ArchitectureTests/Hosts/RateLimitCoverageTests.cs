using System.Text.RegularExpressions;
using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Hosts;

/// <summary>
/// Rate limits are decided with every endpoint, not afterwards (docs: security/rate-limiting.md): a public endpoint names
/// a policy, a policy name is one the host registers, and uploads and heavy operations use their own policy. The global
/// soft limit covers everything else.
/// </summary>
public sealed class RateLimitCoverageTests
{
    // Called by Cloud Scheduler with a shared secret, never by a browser; the host leaves /internal unlimited.
    private static readonly string[] InternalEndpoints = ["RunSchedulerTickEndpoint.cs"];

    [Fact]
    public void EveryAnonymousEndpointOrGroup_NamesARateLimitPolicy()
    {
        var violations = EndpointFiles()
            .Where(path => File.ReadAllText(path).Contains(".AllowAnonymous()", StringComparison.Ordinal))
            .Where(path => !File.ReadAllText(path).Contains("RequireRateLimiting(", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .Where(name => !InternalEndpoints.Contains(name))
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void EveryNamedPolicy_IsRegisteredByTheHost()
    {
        var registered = Regex
            .Matches(File.ReadAllText(Path.Combine(HostNetwork(), "RateLimitingRegistrationExtensions.cs")), "AddPolicy\\(\"(\\w+)\"")
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        var violations = EndpointFiles()
            .SelectMany(path => Regex.Matches(File.ReadAllText(path), "RequireRateLimiting\\(\"(\\w+)\"\\)")
                .Select(match => (File: Path.GetFileName(path), Policy: match.Groups[1].Value)))
            .Where(use => !registered.Contains(use.Policy))
            .Select(use => $"{use.File}: {use.Policy}")
            .Order()
            .ToArray();

        Assert.NotEmpty(registered);
        Assert.Empty(violations);
    }

    [Fact]
    public void UploadsAndHeavyOperations_UseTheirOwnPolicy()
    {
        var violations = EndpointFiles()
            .Select(path => (Name: Path.GetFileName(path), Text: File.ReadAllText(path)))
            .Where(file => file.Name.EndsWith("Endpoint.cs", StringComparison.Ordinal))
            .Select(file => (file.Name, Expected: ExpectedPolicy(file.Name), file.Text))
            .Where(file => file.Expected is not null)
            .Where(file => !file.Text.Contains($"RequireRateLimiting(\"{file.Expected}\")", StringComparison.Ordinal))
            .Select(file => $"{file.Name}: {file.Expected}")
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    private static string? ExpectedPolicy(string fileName) =>
        fileName.StartsWith("Upload", StringComparison.Ordinal) ? "Upload"
        : Regex.IsMatch(fileName, "Import|Report|Pdf|Export|Snapshot|RetryOrderSync") ? "HeavyOperation"
        : null;

    private static string HostNetwork() =>
        Path.Combine(RepositoryRoot.Find(), "src", "Bootstrapper", "Host.Api", "Network");

    private static IEnumerable<string> EndpointFiles() =>
        Directory.EnumerateFiles(Path.Combine(RepositoryRoot.Find(), "src", "Modules"), "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains("Endpoints"))
            .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(s => s is "bin" or "obj"));
}
