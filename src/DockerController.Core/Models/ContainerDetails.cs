namespace DockerController.Core.Models;

public sealed record ContainerDetails(
    string Id,
    string Name,
    string Image,
    string ImageId,
    ContainerState State,
    string StatusText,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    TimeSpan? Uptime,
    int RestartCount,
    string? Command,
    IReadOnlyList<PortMapping> Ports,
    IReadOnlyList<string> Networks,
    IReadOnlyDictionary<string, string> Labels);
