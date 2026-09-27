namespace DockerController.Core.Security;

public sealed record ContainerAccessSubject(
    string Id,
    IReadOnlyList<string> Names,
    IReadOnlyDictionary<string, string> Labels);
