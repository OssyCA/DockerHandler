namespace DockerController.Core.Configuration;

public sealed class ApiKeyEntry
{
    public string Id { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public const int MinKeyLength = 32;

    public ApiKeyEntry[] Keys { get; set; } = [];

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (Keys.Length == 0)
        {
            errors.Add($"{nameof(Keys)} innehåller inga nycklar. Minst en krävs, annars går API:et inte att nå.");
            return errors;
        }

        if (Keys.Any(key => string.IsNullOrWhiteSpace(key.Id)))
        {
            errors.Add($"{nameof(Keys)} innehåller en post utan {nameof(ApiKeyEntry.Id)}.");
        }

        var duplicateIds = Keys
            .Select(key => key.Id.Trim())
            .Where(id => id.Length > 0)
            .GroupBy(id => id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            errors.Add($"{nameof(Keys)} har dubblerade {nameof(ApiKeyEntry.Id)}: {string.Join(", ", duplicateIds)}.");
        }

        var tooShort = Keys
            .Where(key => key.Value.Length < MinKeyLength)
            .Select(key => Describe(key.Id))
            .ToArray();

        if (tooShort.Length > 0)
        {
            errors.Add($"{nameof(Keys)} har nycklar kortare än {MinKeyLength} tecken: {string.Join(", ", tooShort)}.");
        }

        var sharedValueIds = Keys
            .GroupBy(key => key.Value, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(key => Describe(key.Id)))
            .ToArray();

        if (sharedValueIds.Length > 0)
        {
            errors.Add($"{nameof(Keys)} har samma {nameof(ApiKeyEntry.Value)} på flera poster: {string.Join(", ", sharedValueIds)}.");
        }

        return errors;
    }

    private static string Describe(string id) => string.IsNullOrWhiteSpace(id) ? "(utan id)" : id.Trim();
}
