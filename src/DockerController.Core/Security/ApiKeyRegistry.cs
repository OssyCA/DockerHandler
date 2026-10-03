using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using DockerController.Core.Configuration;

namespace DockerController.Core.Security;

public sealed class ApiKeyRegistry
{
    private readonly (string Id, byte[] Hash)[] _keys;

    public ApiKeyRegistry(AuthOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _keys = options.Keys
            .Select(key => (key.Id.Trim(), Hash(key.Value)))
            .ToArray();
    }

    public bool TryResolve(string presented, [NotNullWhen(true)] out string? keyId)
    {
        var candidate = Hash(presented);
        keyId = null;

        foreach (var (id, hash) in _keys)
        {
            if (CryptographicOperations.FixedTimeEquals(candidate, hash))
            {
                keyId = id;
            }
        }

        return keyId is not null;
    }

    private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
}
