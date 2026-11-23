using DockerController.Core.Models;

namespace DockerController.Core.Abstractions;

public interface IDockerClientAdapter
{
    IAsyncEnumerable<ContainerListing> ListContainersAsync(
        LabelFilter? filter,
        CancellationToken cancellationToken);

    Task<ContainerInspection?> InspectContainerAsync(
        string idOrName,
        CancellationToken cancellationToken);

    Task<ContainerOperationOutcome> StartContainerAsync(string id, CancellationToken cancellationToken);

    Task<ContainerOperationOutcome> StopContainerAsync(
        string id,
        int timeoutSeconds,
        CancellationToken cancellationToken);

    Task<ContainerOperationOutcome> RestartContainerAsync(
        string id,
        int timeoutSeconds,
        CancellationToken cancellationToken);

    IAsyncEnumerable<ImageSummary> ListImagesAsync(CancellationToken cancellationToken);

    Task<DaemonHealth> CheckDaemonAsync(CancellationToken cancellationToken);
}
