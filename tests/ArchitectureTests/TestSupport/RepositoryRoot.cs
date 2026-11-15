namespace ArchitectureTests.TestSupport;

/// <summary>The folder that holds <c>KedrStore.sln</c>, found above the test output directory.</summary>
internal static class RepositoryRoot
{
    public static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("KedrStore.sln was not found above the test output directory.");
    }
}
