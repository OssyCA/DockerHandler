using DockerController.Core.Configuration;

namespace DockerController.Core.Tests;

public class DockerOptionsTests
{
    private static DockerOptions Valid() => new()
    {
        Endpoint = "unix:///var/run/docker.sock",
        DeniedNames = ["docker-controller"],
    };

    [Fact]
    public void Accepts_supported_endpoint()
    {
        Assert.Empty(Valid().Validate());
    }

    [Theory]
    [InlineData("ftp://somewhere")]
    [InlineData("not-a-uri")]
    public void Rejects_unsupported_endpoint(string endpoint)
    {
        var options = Valid();
        options.Endpoint = endpoint;

        var error = Assert.Single(options.Validate());
        Assert.Contains(nameof(DockerOptions.Endpoint), error, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_duplicate_denied_names_ignoring_slash_and_case()
    {
        var options = Valid();
        options.DeniedNames = ["docker-controller", "/Docker-Controller"];

        var error = Assert.Single(options.Validate());
        Assert.Contains(nameof(DockerOptions.DeniedNames), error, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_label_key_containing_equals_sign()
    {
        var options = Valid();
        options.ManagedLabelKey = "managed-by=x";

        var error = Assert.Single(options.Validate());
        Assert.Contains(nameof(DockerOptions.ManagedLabelKey), error, StringComparison.Ordinal);
    }
}
