namespace CustomerApi.Contracts.Requests;

public sealed record CreateApiKeyRequest(
    string Name,
    string[] Roles,
    DateTimeOffset? ExpiresAtUtc);
