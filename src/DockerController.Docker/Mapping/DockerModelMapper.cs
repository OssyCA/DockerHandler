using System.Collections.ObjectModel;
using System.Globalization;
using Docker.DotNet.Models;
using DockerController.Core.Models;
using DockerController.Core.Security;

namespace DockerController.Docker.Mapping;

internal static class DockerModelMapper
{
    public static ContainerSummary ToSummary(
        ContainerListResponse response,
        DateTimeOffset? startedAt,
        DateTimeOffset now) =>
        new(
            response.ID,
            PrimaryName(response.Names),
            response.Image,
            ParseState(response.State),
            response.Status ?? string.Empty,
            ToUtc(response.Created),
            startedAt,
            startedAt is null ? null : now - startedAt.Value,
            response.Ports?.Select(ToPortMapping).ToArray() ?? []);

    public static ContainerDetails ToDetails(ContainerInspectResponse response, DateTimeOffset now)
    {
        var state = ParseState(response.State?.Status);
        var running = state == ContainerState.Running;
        var startedAt = ParseTimestamp(response.State?.StartedAt);

        return new ContainerDetails(
            response.ID,
            StripSlash(response.Name),
            response.Config?.Image ?? string.Empty,
            response.Image ?? string.Empty,
            state,
            response.State?.Status ?? string.Empty,
            ToUtc(response.Created),
            startedAt,
            running ? null : ParseTimestamp(response.State?.FinishedAt),
            running && startedAt is not null ? now - startedAt.Value : null,
            (int)response.RestartCount,
            BuildCommand(response),
            ToPortMappings(response.NetworkSettings?.Ports),
            response.NetworkSettings?.Networks?.Keys.ToArray() ?? [],
            ToLabels(response.Config?.Labels));
    }

    public static ImageSummary ToImageSummary(ImagesListResponse response) =>
        new(
            response.ID,
            response.RepoTags?.Where(tag => tag != "<none>:<none>").ToArray() ?? [],
            response.Size,
            ToUtc(response.Created));

    public static ContainerAccessSubject ToSubject(ContainerListResponse response) =>
        new(response.ID, response.Names?.ToArray() ?? [], ToLabels(response.Labels));

    public static ContainerAccessSubject ToSubject(ContainerInspectResponse response) =>
        new(response.ID, [response.Name ?? string.Empty], ToLabels(response.Config?.Labels));

    public static DateTimeOffset? ParseTimestamp(string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed)
        && parsed.Year > 1
            ? parsed
            : null;

    public static ContainerState ParseState(string? state) => state switch
    {
        "created" => ContainerState.Created,
        "running" => ContainerState.Running,
        "paused" => ContainerState.Paused,
        "restarting" => ContainerState.Restarting,
        "removing" => ContainerState.Removing,
        "exited" => ContainerState.Exited,
        "dead" => ContainerState.Dead,
        _ => ContainerState.Unknown,
    };

    private static IReadOnlyDictionary<string, string> ToLabels(IDictionary<string, string>? labels) =>
        labels is null ? ReadOnlyDictionary<string, string>.Empty : labels.AsReadOnly();

    private static DateTimeOffset ToUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static string PrimaryName(IList<string>? names) =>
        names is { Count: > 0 } ? StripSlash(names[0]) : string.Empty;

    private static string StripSlash(string? name) => name?.TrimStart('/') ?? string.Empty;

    private static string? BuildCommand(ContainerInspectResponse response) =>
        response.Config?.Cmd is { Count: > 0 } cmd
            ? string.Join(' ', cmd)
            : string.IsNullOrEmpty(response.Path) ? null : response.Path;

    private static PortMapping ToPortMapping(PortSummary port) =>
        new(port.PrivatePort, port.PublicPort, ParseProtocol(port.Type), port.IP);

    private static IReadOnlyList<PortMapping> ToPortMappings(IDictionary<string, IList<PortBinding>>? ports)
    {
        if (ports is null)
        {
            return [];
        }

        var mappings = new List<PortMapping>();

        foreach (var (portAndProtocol, bindings) in ports)
        {
            var parts = portAndProtocol.Split('/');

            if (!int.TryParse(parts[0], CultureInfo.InvariantCulture, out var privatePort))
            {
                continue;
            }

            var protocol = ParseProtocol(parts.Length > 1 ? parts[1] : null);

            if (bindings is null || bindings.Count == 0)
            {
                mappings.Add(new PortMapping(privatePort, null, protocol, null));
                continue;
            }

            mappings.AddRange(bindings.Select(binding => new PortMapping(
                privatePort,
                int.TryParse(binding.HostPort, CultureInfo.InvariantCulture, out var hostPort) ? hostPort : null,
                protocol,
                binding.HostIP)));
        }

        return mappings;
    }

    private static PortProtocol ParseProtocol(string? protocol) => protocol switch
    {
        "tcp" => PortProtocol.Tcp,
        "udp" => PortProtocol.Udp,
        "sctp" => PortProtocol.Sctp,
        _ => PortProtocol.Unknown,
    };
}
