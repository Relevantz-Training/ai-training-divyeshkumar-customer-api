using System.ComponentModel.DataAnnotations;

namespace CustomerApi.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = "CustomerApi";

    [Required]
    public string Audience { get; init; } = "CustomerApiClients";

    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = "local-dev-signing-key-change-this-value-now";

    [Range(5, 480)]
    public int TokenLifetimeMinutes { get; init; } = 60;
}
