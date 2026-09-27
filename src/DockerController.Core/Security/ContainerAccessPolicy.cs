using System.Collections.Frozen;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;
using DockerController.Core.Models;

namespace DockerController.Core.Security;

public sealed class ContainerAccessPolicy : IContainerAccessPolicy
{
    private readonly string _labelKey;
    private readonly string _labelValue;
    private readonly FrozenSet<string> _deniedNames;

    public ContainerAccessPolicy(DockerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _labelKey = options.ManagedLabelKey;
        _labelValue = options.ManagedLabelValue;

        // Skiftlägesokänsligt är strängare än Dockers jämförelse: att neka för brett
        // är rätt håll att fela på.
        _deniedNames = options.DeniedNames
            .Select(StripNamePrefix)
            .Where(name => name.Length > 0)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public LabelFilter ManagedLabel => new(_labelKey, _labelValue);

    public AccessDecision Evaluate(ContainerAccessSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        // Denylistan vinner över labeln: annars kan controllern ge sig själv åtkomst
        // genom att sätta labeln på sig själv. Namnen kommer från daemonen, inte från
        // begäran, så det hjälper inte att ange id i stället för namn.
        foreach (var name in subject.Names)
        {
            if (_deniedNames.Contains(StripNamePrefix(name)))
            {
                return AccessDecision.DeniedByName;
            }
        }

        if (!subject.Labels.TryGetValue(_labelKey, out var value) ||
            !string.Equals(value, _labelValue, StringComparison.Ordinal))
        {
            return AccessDecision.DeniedNotManaged;
        }

        return AccessDecision.Allowed;
    }

    private static string StripNamePrefix(string name) => name.TrimStart('/').Trim();
}
