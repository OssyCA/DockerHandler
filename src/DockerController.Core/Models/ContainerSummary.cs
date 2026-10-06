namespace DockerController.Core.Models;

public sealed record ContainerSummary(
    string Id,
    string Name,
    string Image,
    ContainerState State,
    string StatusText,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    TimeSpan? Uptime,
    IReadOnlyList<PortMapping> Ports);
