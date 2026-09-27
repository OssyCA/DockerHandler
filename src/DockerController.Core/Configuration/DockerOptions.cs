using System.ComponentModel.DataAnnotations;

namespace DockerController.Core.Configuration;

public sealed class DockerOptions
{
    public const string SectionName = "Docker";

    public const int MinStopTimeoutSeconds = 0;
    public const int MaxStopTimeoutSeconds = 300;

    private static readonly string[] AllowedSchemes = ["npipe", "unix", "tcp", "http", "https"];

    [Required(AllowEmptyStrings = false)]
    public string Endpoint { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string ManagedLabelKey { get; set; } = "managed-by";

    [Required(AllowEmptyStrings = false)]
    public string ManagedLabelValue { get; set; } = "docker-controller";

    public string[] DeniedNames { get; set; } = [];

    [Range(MinStopTimeoutSeconds, MaxStopTimeoutSeconds)]
    public int DefaultStopTimeoutSeconds { get; set; } = 10;

    [Range(1, 120)]
    public int ConnectionTimeoutSeconds { get; set; } = 10;

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var uri))
        {
            errors.Add($"{nameof(Endpoint)} är inte en absolut URI: '{Endpoint}'.");
        }
        else if (!AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"{nameof(Endpoint)} har schemat '{uri.Scheme}'. Tillåtna: {string.Join(", ", AllowedSchemes)}.");
        }

        if (ManagedLabelKey.AsSpan().ContainsAny(' ', '='))
        {
            errors.Add($"{nameof(ManagedLabelKey)} får inte innehålla blanksteg eller likhetstecken.");
        }

        if (DeniedNames.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add($"{nameof(DeniedNames)} innehåller ett tomt namn.");
        }

        var duplicates = DeniedNames
            .Select(name => name.TrimStart('/').Trim())
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            errors.Add($"{nameof(DeniedNames)} innehåller dubbletter: {string.Join(", ", duplicates)}.");
        }

        return errors;
    }
}
