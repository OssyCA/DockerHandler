using DockerController.Core.Configuration;
using DockerController.Core.Security;

namespace DockerController.Core.Tests;

public class ContainerAccessPolicyTests
{
    private static ContainerAccessPolicy CreatePolicy(params string[] deniedNames) =>
        new(new DockerOptions
        {
            Endpoint = "npipe://./pipe/docker_engine",
            ManagedLabelKey = "managed-by",
            ManagedLabelValue = "docker-controller",
            DeniedNames = deniedNames,
        });

    private static ContainerAccessSubject Subject(
        string name,
        params (string Key, string Value)[] labels) =>
        new("abc123", [name], labels.ToDictionary(label => label.Key, label => label.Value));

    [Fact]
    public void Allows_container_with_managed_label()
    {
        var decision = CreatePolicy().Evaluate(Subject("/web", ("managed-by", "docker-controller")));

        Assert.Equal(AccessDecision.Allowed, decision);
    }

    [Fact]
    public void Denies_container_without_label()
    {
        var decision = CreatePolicy().Evaluate(Subject("/web"));

        Assert.Equal(AccessDecision.DeniedNotManaged, decision);
    }

    [Fact]
    public void Denies_container_with_wrong_label_value()
    {
        var decision = CreatePolicy().Evaluate(Subject("/web", ("managed-by", "someone-else")));

        Assert.Equal(AccessDecision.DeniedNotManaged, decision);
    }

    [Fact]
    public void Label_key_comparison_is_case_sensitive()
    {
        var decision = CreatePolicy().Evaluate(Subject("/web", ("Managed-By", "docker-controller")));

        Assert.Equal(AccessDecision.DeniedNotManaged, decision);
    }

    [Fact]
    public void Denylist_wins_over_managed_label()
    {
        var decision = CreatePolicy("docker-controller")
            .Evaluate(Subject("/docker-controller", ("managed-by", "docker-controller")));

        Assert.Equal(AccessDecision.DeniedByName, decision);
    }

    [Theory]
    [InlineData("docker-controller", "/docker-controller")]
    [InlineData("/Docker-Controller", "docker-controller")]
    public void Denylist_ignores_slash_prefix_and_case(string containerName, string deniedName)
    {
        var decision = CreatePolicy(deniedName)
            .Evaluate(Subject(containerName, ("managed-by", "docker-controller")));

        Assert.Equal(AccessDecision.DeniedByName, decision);
    }

    [Fact]
    public void Denylist_matches_any_of_the_container_aliases()
    {
        var subject = new ContainerAccessSubject(
            "abc123",
            ["/web", "/docker-controller"],
            new Dictionary<string, string> { ["managed-by"] = "docker-controller" });

        Assert.Equal(AccessDecision.DeniedByName, CreatePolicy("docker-controller").Evaluate(subject));
    }
}
