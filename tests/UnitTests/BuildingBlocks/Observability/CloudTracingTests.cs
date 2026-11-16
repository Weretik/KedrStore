using System.Diagnostics;
using BuildingBlocks.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace UnitTests.BuildingBlocks.Observability;

public sealed class CloudTracingTests
{
    [Fact]
    public void WithoutProjectId_NothingIsExported()
    {
        var services = new ServiceCollection();

        CloudTracing.Add(services, new ObservabilityOptions { ServiceName = "kedr-api" });

        using var provider = services.BuildServiceProvider();
        Assert.Null(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void WithProjectId_TracerIsRegistered_AndPassesStartupValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        CloudTracing.Add(services, new ObservabilityOptions { ServiceName = "kedr-api", GoogleCloudProjectId = "classkedrproject" });

        // The hosts validate scopes and every registration at startup.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void Sampler_IgnoresTheNotSampledFlagOfCloudRun()
    {
        var parent = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.None, isRemote: true);
        var parameters = new SamplingParameters(parent, parent.TraceId, "GET /api/catalog", ActivityKind.Server);

        var result = CloudTracing.CreateSampler(1).ShouldSample(parameters);

        Assert.Equal(SamplingDecision.RecordAndSample, result.Decision);
    }

    [Fact]
    public void Sampler_ZeroRatio_DropsEverything()
    {
        var parameters = new SamplingParameters(default, ActivityTraceId.CreateRandom(), "GET /api/catalog", ActivityKind.Server);

        Assert.Equal(SamplingDecision.Drop, CloudTracing.CreateSampler(0).ShouldSample(parameters).Decision);
    }

    [Theory]
    [InlineData("/health", false)]
    [InlineData("/api/catalog/categories", true)]
    public void HealthCheck_IsNotTraced(string path, bool traced)
    {
        var context = new DefaultHttpContext { Request = { Path = path } };

        Assert.Equal(traced, CloudTracing.ShouldTrace(context));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void SampleRatio_OutsideZeroToOne_IsRejected(double ratio)
        => Assert.Throws<ArgumentOutOfRangeException>(() => CloudTracing.CreateSampler(ratio));
}
