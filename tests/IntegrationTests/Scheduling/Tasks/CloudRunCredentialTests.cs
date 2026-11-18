using Google.Apis.Auth.OAuth2;
using Scheduling.Infrastructure.Integrations.CloudRun;

namespace IntegrationTests.Scheduling.Tasks;

/// <summary>No Docker: the launcher's credential on Cloud Run skips the cached ADC metadata ping.</summary>
public sealed class CloudRunCredentialTests
{
    [Theory]
    [InlineData("K_SERVICE", "kedr-api")]
    [InlineData("CLOUD_RUN_JOB", "kedr-jobs")]
    public async Task OnCloudRun_TheCredentialIsTheMetadataServers(string variable, string value)
    {
        var credential = await CloudRunCredential.GetAsync(name => name == variable ? value : null, CancellationToken.None);

        Assert.IsType<ComputeCredential>(credential.UnderlyingCredential);
    }

    [Fact]
    public void OutsideCloudRun_ItIsNotTreatedAsCloudRun()
    {
        Assert.False(CloudRunCredential.IsCloudRun(_ => null));
    }
}
