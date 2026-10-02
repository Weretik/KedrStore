namespace IntegrationTests.Platform.Api;

public sealed class MissingProductPhotoContractTests
{
    [Fact]
    public void VersionedAndAggregateContractsDeclareAnonymousMissingPhotoRead()
    {
        var root = FindRepositoryRoot();
        var catalogContract = File.ReadAllText(Path.Combine(
            root, "docs", "sdd", "contracts", "catalog", "products.openapi.yaml"));
        var aggregateContract = File.ReadAllText(Path.Combine(
            root, "docs", "sdd", "contracts", "openapi.yaml"));

        Assert.Contains("/api/admin/products/missing-photos:", catalogContract, StringComparison.Ordinal);
        Assert.Contains("operationId: getAdminProductsWithUnavailablePhotos", catalogContract, StringComparison.Ordinal);
        Assert.Contains("security: []", catalogContract, StringComparison.Ordinal);
        Assert.Contains("enum: [Missing, InvalidContentType, CheckFailed]", catalogContract, StringComparison.Ordinal);
        Assert.Contains("const: false", catalogContract, StringComparison.Ordinal);
        Assert.Contains("./catalog/products.openapi.yaml#/paths/~1api~1admin~1products~1missing-photos", aggregateContract,
            StringComparison.Ordinal);
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
