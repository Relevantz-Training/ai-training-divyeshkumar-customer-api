namespace CustomerApi.Domain.Models;

public sealed class ApiKey
{
    public Guid Id { get; set; }

    /// <summary>Human-readable label for the key (e.g. "Mobile App", "Partner X").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>First 8 characters of the raw key, stored in plaintext for fast DB lookup.</summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>SHA-256 hex hash of the full raw key. Never store the raw key.</summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>Comma-separated roles granted to this key, e.g. "Admin" or "Support".</summary>
    public string Roles { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? ExpiresAtUtc { get; set; }
}
