using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using CustomerApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CustomerApi.Security;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceScopeFactory scopeFactory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var rawKeyValues))
        {
            return AuthenticateResult.NoResult();
        }

        var rawKey = rawKeyValues.ToString().Trim();
        if (rawKey.Length < 8)
        {
            return AuthenticateResult.Fail("Invalid API key format.");
        }

        var prefix = rawKey[..8];
        var expectedHash = ComputeHash(rawKey);

        // Resolve scoped DbContext from a new scope (handler is singleton-compatible)
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        var apiKey = await dbContext.ApiKeys
            .Where(k => k.KeyPrefix == prefix && k.IsActive)
            .FirstOrDefaultAsync(Context.RequestAborted);

        if (apiKey is null)
        {
            return AuthenticateResult.Fail("API key not found or inactive.");
        }

        if (apiKey.ExpiresAtUtc.HasValue && apiKey.ExpiresAtUtc.Value < DateTimeOffset.UtcNow)
        {
            return AuthenticateResult.Fail("API key has expired.");
        }

        // Constant-time compare to prevent timing attacks
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedHash),
                Encoding.UTF8.GetBytes(apiKey.KeyHash)))
        {
            return AuthenticateResult.Fail("API key is invalid.");
        }

        var roles = apiKey.Roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, apiKey.Name),
            new(ClaimTypes.NameIdentifier, apiKey.Id.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    public static string ComputeHash(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string GenerateRawKey()
    {
        // 32 random bytes → 64-char hex string, URL-safe
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
