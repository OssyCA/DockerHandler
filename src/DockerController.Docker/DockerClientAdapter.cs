using System.Runtime.CompilerServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerController.Core.Abstractions;
using DockerController.Core.Exceptions;
using DockerController.Core.Models;
using DockerController.Docker.Mapping;
using Microsoft.Extensions.Logging;

namespace DockerController.Docker;

public sealed class DockerClientAdapter(
    DockerClient client,
    TimeProvider timeProvider,
    ILogger<DockerClientAdapter> logger) : IDockerClientAdapter
{
    public async IAsyncEnumerable<ContainerListing> ListContainersAsync(
        LabelFilter? filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var parameters = new ContainersListParameters { All = true };

        if (filter is { } label)
        {
            parameters.Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                ["label"] = new Dictionary<string, bool> { [$"{label.Key}={label.Value}"] = true },
            };
        }

        var responses = await ExecuteAsync(
            () => client.Containers.ListContainersAsync(parameters, cancellationToken),
            cancellationToken);

        foreach (var response in responses)
        {
            var startedAt = await ReadStartedAtAsync(response, cancellationToken);

            yield return MapResponse(() => new ContainerListing(
                DockerModelMapper.ToSummary(response, startedAt, timeProvider.GetUtcNow()),
                DockerModelMapper.ToSubject(response)));
        }
    }

    public async Task<ContainerInspection?> InspectContainerAsync(
        string idOrName,
        CancellationToken cancellationToken)
    {
        var response = await InspectOrNullAsync(idOrName, cancellationToken);

        return response is null
            ? null
            : MapResponse(() => new ContainerInspection(
                DockerModelMapper.ToDetails(response, timeProvider.GetUtcNow()),
                DockerModelMapper.ToSubject(response)));
    }

    public Task<ContainerOperationOutcome> StartContainerAsync(string id, CancellationToken cancellationToken) =>
        ChangeStateAsync(
            () => client.Containers.StartContainerAsync(id, new ContainerStartParameters(), cancellationToken),
            cancellationToken);

    public Task<ContainerOperationOutcome> StopContainerAsync(
        string id,
        int timeoutSeconds,
        CancellationToken cancellationToken) =>
        ChangeStateAsync(
            () => client.Containers.StopContainerAsync(
                id,
                new ContainerStopParameters { WaitBeforeKillSeconds = (uint)timeoutSeconds },
                cancellationToken),
            cancellationToken);

    public async Task<ContainerOperationOutcome> RestartContainerAsync(
        string id,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteAsync(
                () => client.Containers.RestartContainerAsync(
                    id,
                    new ContainerRestartParameters { WaitBeforeKillSeconds = (uint)timeoutSeconds },
                    cancellationToken),
                cancellationToken);

            return ContainerOperationOutcome.Changed;
        }
        catch (DockerContainerNotFoundException)
        {
            return ContainerOperationOutcome.NotFound;
        }
    }

    public async IAsyncEnumerable<ImageSummary> ListImagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var responses = await ExecuteAsync(
            () => client.Images.ListImagesAsync(new ImagesListParameters(), cancellationToken),
            cancellationToken);

        foreach (var response in responses)
        {
            yield return MapResponse(() => DockerModelMapper.ToImageSummary(response));
        }
    }

    public async Task<DaemonHealth> CheckDaemonAsync(CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();

        try
        {
            var version = await client.System.GetVersionAsync(cancellationToken);

            return new DaemonHealth(
                HealthStatus.Healthy,
                DaemonReachable: true,
                version.APIVersion,
                version.Version,
                timeProvider.GetElapsedTime(startedAt),
                timeProvider.GetUtcNow(),
                Detail: null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Daemonen svarade inte på {DockerEndpoint}", client.Options.Endpoint);

            return new DaemonHealth(
                HealthStatus.Unhealthy,
                DaemonReachable: false,
                ApiVersion: null,
                ServerVersion: null,
                timeProvider.GetElapsedTime(startedAt),
                timeProvider.GetUtcNow(),
                "Docker-daemonen svarar inte.");
        }
    }

    private async Task<DateTimeOffset?> ReadStartedAtAsync(
        ContainerListResponse response,
        CancellationToken cancellationToken)
    {
        if (DockerModelMapper.ParseState(response.State) != ContainerState.Running)
        {
            return null;
        }

        var inspected = await InspectOrNullAsync(response.ID, cancellationToken);

        return DockerModelMapper.ParseTimestamp(inspected?.State?.StartedAt);
    }

    private async Task<ContainerInspectResponse?> InspectOrNullAsync(
        string idOrName,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ExecuteAsync(
                () => client.Containers.InspectContainerAsync(idOrName, cancellationToken),
                cancellationToken);
        }
        catch (DockerContainerNotFoundException)
        {
            return null;
        }
    }

    private async Task<ContainerOperationOutcome> ChangeStateAsync(
        Func<Task<bool>> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ExecuteAsync(operation, cancellationToken)
                ? ContainerOperationOutcome.Changed
                : ContainerOperationOutcome.AlreadyInDesiredState;
        }
        catch (DockerContainerNotFoundException)
        {
            return ContainerOperationOutcome.NotFound;
        }
    }

    private static T MapResponse<T>(Func<T> map)
    {
        try
        {
            return map();
        }
        catch (Exception ex)
        {
            throw new DockerProtocolException("Kunde inte tolka svaret från Docker-daemonen.", ex);
        }
    }

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            throw DockerExceptionTranslator.Translate(ex, client.Options.Endpoint, cancellationToken);
        }
    }

    private async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            throw DockerExceptionTranslator.Translate(ex, client.Options.Endpoint, cancellationToken);
        }
    }
}
