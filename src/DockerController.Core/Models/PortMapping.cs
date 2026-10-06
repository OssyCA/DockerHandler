namespace DockerController.Core.Models;

public sealed record PortMapping(
    int PrivatePort,
    int? PublicPort,
    PortProtocol Protocol,
    string? HostIp);
