namespace DockerController.Core.Models;

public sealed record DaemonHealth(
    HealthStatus Status,
    bool DaemonReachable,
    string? ApiVersion,
    string? ServerVersion,
    TimeSpan? Latency,
    DateTimeOffset CheckedAt,
    // Kort orsak, aldrig undantagstexter eller sökvägar: fältet går ut i svaret.
    string? Detail);
