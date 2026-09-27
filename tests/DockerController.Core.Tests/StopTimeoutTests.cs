using DockerController.Core.Results;
using DockerController.Core.Validation;

namespace DockerController.Core.Tests;

public class StopTimeoutTests
{
    [Fact]
    public void Falls_back_to_configured_default()
    {
        var result = StopTimeout.Resolve(requestedSeconds: null, configuredDefaultSeconds: 15);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(300)]
    public void Accepts_interval_bounds(int seconds)
    {
        var result = StopTimeout.Resolve(seconds, configuredDefaultSeconds: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(seconds, result.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(301)]
    public void Rejects_values_outside_interval(int seconds)
    {
        var result = StopTimeout.Resolve(seconds, 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultError.Validation, result.Error);
    }
}
