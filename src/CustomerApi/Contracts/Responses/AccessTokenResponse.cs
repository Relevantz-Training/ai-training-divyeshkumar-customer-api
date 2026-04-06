namespace CustomerApi.Contracts.Responses;

public sealed class AccessTokenResponse
{
    public string TokenType { get; init; } = "Bearer";

    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public string Role { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}
