namespace IntegrationTests.Platform.Jobs;

public sealed class ProductPhotoCheckDeploymentTests
{
    [Fact]
    public void HostAndWorkflow_DeclareOneShotPhotoCheckWithoutExecutionOrScheduler()
    {
        var root = FindRepositoryRoot();
        var hostProgram = File.ReadAllText(Path.Combine(
            root, "src", "Bootstrapper", "Host.Jobs", "Host.Jobs", "Program.cs"));
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "deploy-cloudrun.yml"));

        Assert.Contains("case \"check-product-photos\"", hostProgram, StringComparison.Ordinal);
        Assert.Contains("PHOTO_CHECK_JOB=\"catalog-product-photo-check\"", workflow, StringComparison.Ordinal);
        Assert.Contains("PHOTO_CHECK_ARGS=\"--job=check-product-photos\"", workflow, StringComparison.Ordinal);
        Assert.Contains("--tasks 1", workflow, StringComparison.Ordinal);
        Assert.Contains("--parallelism 1", workflow, StringComparison.Ordinal);
        Assert.Contains("--max-retries 0", workflow, StringComparison.Ordinal);
        Assert.Contains("--task-timeout 30m", workflow, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__Default=db-connection:latest", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("jobs execute \"$PHOTO_CHECK_JOB\"", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("scheduler jobs create", workflow, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
