using System.Runtime.CompilerServices;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;
using DockerController.Core.Models;
using DockerController.Core.Results;
using DockerController.Core.Security;
using DockerController.Docker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DockerController.Core.Tests;

public class ContainerServiceTests
{
    private const string ManagedLabelKey = "managed-by";
    private const string ManagedLabelValue = "docker-controller";

    [Fact]
    public async Task List_yields_only_containers_the_policy_allows()
    {
        var adapter = new FakeAdapter
        {
            Containers =
            [
                Listing("managed", labelled: true),
                Listing("unlabelled", labelled: false),
                Listing("docker-controller", labelled: true),
            ],
        };

        var names = new List<string>();

        await foreach (var summary in CreateService(adapter).ListManagedAsync(CancellationToken.None))
        {
            names.Add(summary.Name);
        }

        Assert.Equal(["managed"], names);
    }

    [Fact]
    public async Task Get_returns_not_found_for_unmanaged_container()
    {
        var adapter = new FakeAdapter { Inspected = Inspection("unlabelled", labelled: false) };

        var result = await CreateService(adapter).GetAsync("unlabelled", CancellationToken.None);

        Assert.Equal(ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task Get_returns_the_same_message_for_missing_and_unmanaged()
    {
        var missing = await CreateService(new FakeAdapter())
            .GetAsync("gone", CancellationToken.None);

        var unmanaged = await CreateService(new FakeAdapter { Inspected = Inspection("x", labelled: false) })
            .GetAsync("x", CancellationToken.None);

        Assert.Equal(missing.Message, unmanaged.Message);
    }

    [Fact]
    public async Task Start_is_never_attempted_on_a_denied_container()
    {
        var adapter = new FakeAdapter { Inspected = Inspection("docker-controller", labelled: true) };

        var result = await CreateService(adapter).StartAsync("docker-controller", CancellationToken.None);

        Assert.Equal(ResultError.NotFound, result.Error);
        Assert.Empty(adapter.StartedIds);
    }

    [Fact]
    public async Task Stop_rejects_an_invalid_timeout_before_touching_the_daemon()
    {
        var adapter = new FakeAdapter { Inspected = Inspection("managed", labelled: true) };

        var result = await CreateService(adapter).StopAsync("managed", 301, CancellationToken.None);

        Assert.Equal(ResultError.Validation, result.Error);
        Assert.Empty(adapter.StoppedIds);
    }

    [Fact]
    public async Task Stop_falls_back_to_the_configured_timeout()
    {
        var adapter = new FakeAdapter { Inspected = Inspection("managed", labelled: true) };

        await CreateService(adapter, defaultStopTimeoutSeconds: 42)
            .StopAsync("managed", null, CancellationToken.None);

        Assert.Equal([42], adapter.StopTimeouts);
    }

    [Fact]
    public async Task Stop_reports_conflict_when_already_stopped()
    {
        var adapter = new FakeAdapter
        {
            Inspected = Inspection("managed", labelled: true),
            Outcome = ContainerOperationOutcome.AlreadyInDesiredState,
        };

        var result = await CreateService(adapter).StopAsync("managed", null, CancellationToken.None);

        Assert.Equal(ResultError.Conflict, result.Error);
    }

    [Fact]
    public async Task Start_reports_not_found_when_the_container_vanishes_after_inspect()
    {
        var adapter = new FakeAdapter
        {
            Inspected = Inspection("managed", labelled: true),
            Outcome = ContainerOperationOutcome.NotFound,
        };

        var result = await CreateService(adapter).StartAsync("managed", CancellationToken.None);

        Assert.Equal(ResultError.NotFound, result.Error);
    }

    private static ContainerService CreateService(FakeAdapter adapter, int defaultStopTimeoutSeconds = 10)
    {
        var options = new DockerOptions
        {
            Endpoint = "npipe://./pipe/docker_engine",
            ManagedLabelKey = ManagedLabelKey,
            ManagedLabelValue = ManagedLabelValue,
            DeniedNames = ["docker-controller"],
            DefaultStopTimeoutSeconds = defaultStopTimeoutSeconds,
        };

        return new ContainerService(
            adapter,
            new ContainerAccessPolicy(options),
            Options.Create(options),
            NullLogger<ContainerService>.Instance);
    }

    private static ContainerListing Listing(string name, bool labelled) =>
        new(Summary(name), Subject(name, labelled));

    private static ContainerInspection Inspection(string name, bool labelled) =>
        new(Details(name), Subject(name, labelled));

    private static ContainerAccessSubject Subject(string name, bool labelled) =>
        new(
            $"id-{name}",
            [$"/{name}"],
            labelled
                ? new Dictionary<string, string> { [ManagedLabelKey] = ManagedLabelValue }
                : new Dictionary<string, string>());

    private static ContainerSummary Summary(string name) =>
        new($"id-{name}", name, "alpine", ContainerState.Running, "Up", DateTimeOffset.UnixEpoch, null, null, []);

    private static ContainerDetails Details(string name) =>
        new(
            $"id-{name}",
            name,
            "alpine",
            "sha256:x",
            ContainerState.Running,
            "running",
            DateTimeOffset.UnixEpoch,
            null,
            null,
            null,
            0,
            null,
            [],
            [],
            new Dictionary<string, string>());

    private sealed class FakeAdapter : IDockerClientAdapter
    {
        public IReadOnlyList<ContainerListing> Containers { get; init; } = [];

        public ContainerInspection? Inspected { get; init; }

        public ContainerOperationOutcome Outcome { get; init; } = ContainerOperationOutcome.Changed;

        public List<string> StartedIds { get; } = [];

        public List<string> StoppedIds { get; } = [];

        public List<int> StopTimeouts { get; } = [];

        public async IAsyncEnumerable<ContainerListing> ListContainersAsync(
            LabelFilter? filter,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var listing in Containers)
            {
                yield return listing;
            }

            await Task.CompletedTask;
        }

        public Task<ContainerInspection?> InspectContainerAsync(
            string idOrName,
            CancellationToken cancellationToken) => Task.FromResult(Inspected);

        public Task<ContainerOperationOutcome> StartContainerAsync(string id, CancellationToken cancellationToken)
        {
            StartedIds.Add(id);
            return Task.FromResult(Outcome);
        }

        public Task<ContainerOperationOutcome> StopContainerAsync(
            string id,
            int timeoutSeconds,
            CancellationToken cancellationToken)
        {
            StoppedIds.Add(id);
            StopTimeouts.Add(timeoutSeconds);
            return Task.FromResult(Outcome);
        }

        public Task<ContainerOperationOutcome> RestartContainerAsync(
            string id,
            int timeoutSeconds,
            CancellationToken cancellationToken) => Task.FromResult(Outcome);

        public IAsyncEnumerable<ImageSummary> ListImagesAsync(CancellationToken cancellationToken) =>
            AsyncEnumerable.Empty<ImageSummary>();

        public Task<DaemonHealth> CheckDaemonAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
