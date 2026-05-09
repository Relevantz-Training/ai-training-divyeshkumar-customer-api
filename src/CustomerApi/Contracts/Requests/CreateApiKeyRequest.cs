namespace CustomerApi.Contracts.Requests;

public sealed record CreateApiKeyRequest(
    string Name,
    DateTimeOffset? ExpiresAtUtc);
