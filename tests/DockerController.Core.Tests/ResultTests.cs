using DockerController.Core.Results;

namespace DockerController.Core.Tests;

public class ResultTests
{
    [Fact]
    public void TryGetValue_exposes_value_on_success()
    {
        Assert.True(Result<string>.Success("abc").TryGetValue(out var value));
        Assert.Equal("abc", value);
    }

    [Fact]
    public void TryGetValue_is_false_on_failure()
    {
        Assert.False(Result<string>.NotFound("nope").TryGetValue(out _));
    }

    [Fact]
    public void WithoutValue_keeps_the_error()
    {
        var result = Result<string>.Conflict("redan stoppad").WithoutValue();

        Assert.Equal(ResultError.Conflict, result.Error);
        Assert.Equal("redan stoppad", result.Message);
    }
}
