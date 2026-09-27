using System.ComponentModel.DataAnnotations;

namespace DockerController.Core.Configuration;

public sealed class ApiAuthOptions
{
    public const string SectionName = "ApiAuth";
    public const int MinKeyLength = 32;

    public bool Enabled { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string HeaderName { get; set; } = "X-Api-Key";

    public IList<ApiKeyEntry> Keys { get; set; } = [];

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (!Enabled)
        {
            return errors;
        }

        var active = Keys.Where(key => !key.IsExpired(DateTimeOffset.UtcNow)).ToArray();

        if (active.Length == 0)
        {
            errors.Add($"{SectionName}: auth är påslaget men ingen giltig nyckel är konfigurerad.");
        }

        if (active.Any(key => key.Value.Length < MinKeyLength))
        {
            errors.Add($"{SectionName}: varje nyckel måste vara minst {MinKeyLength} tecken.");
        }

        var duplicates = Keys
            .GroupBy(key => key.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            errors.Add($"{SectionName}: nyckelnamnen måste vara unika. Dubbletter: {string.Join(", ", duplicates)}.");
        }

        return errors;
    }
}

public sealed class ApiKeyEntry
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Value { get; set; } = string.Empty;

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt is { } expires && expires <= now;
}
