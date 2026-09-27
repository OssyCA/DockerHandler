namespace DockerController.Core.Models;

public sealed record ImageSummary(
    string Id,
    IReadOnlyList<string> Tags,
    long SizeBytes,
    DateTimeOffset CreatedAt);
