namespace DockerController.Core.Models;

public sealed record DaemonHealth(
    HealthStatus Status,
    bool DaemonReachable,
    string? ApiVersion,
    string? ServerVersion,
    TimeSpan? Latency,
    DateTimeOffset CheckedAt,
    string? Detail);
