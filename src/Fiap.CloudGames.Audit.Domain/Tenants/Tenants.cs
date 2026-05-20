namespace Fiap.CloudGames.Audit.Domain.Tenants;

public static class Tenants
{
    public const string Fiap = "FIAP";
    public const string Alura = "Alura";
    public const string Pm3 = "PM3";
    public const string Unknown = "unknown";

    public static readonly IReadOnlySet<string> Known =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Fiap, Alura, Pm3 };

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Unknown;
        return Known.Contains(raw) ? raw : Unknown;
    }
}
