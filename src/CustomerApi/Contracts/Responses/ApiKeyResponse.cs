namespace CustomerApi.Contracts.Responses;

public sealed record ApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    string[] Roles,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

/// <summary>Returned only once at creation time. The RawKey is never stored.</summary>
public sealed record CreateApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    string[] Roles,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc,
    string RawKey);
