using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Data;
using CustomerApi.Domain.Models;
using CustomerApi.Exceptions;
using CustomerApi.Security;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Services;

public sealed class ApiKeyService(CustomerDbContext dbContext) : IApiKeyService
{
    // API keys are always read-only — they receive the Support role which grants GET access only.
    private const string ApiKeyRole = "Support";

    public async Task<CreateApiKeyResponse> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken)
    {
        var rawKey = ApiKeyAuthenticationHandler.GenerateRawKey();
        var prefix = rawKey[..8];
        var hash = ApiKeyAuthenticationHandler.ComputeHash(rawKey);

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            KeyPrefix = prefix,
            KeyHash = hash,
            Roles = ApiKeyRole,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = request.ExpiresAtUtc
        };

        dbContext.ApiKeys.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateApiKeyResponse(
            entity.Id,
            entity.Name,
            entity.KeyPrefix,
            [ApiKeyRole],
            entity.IsActive,
            entity.CreatedAtUtc,
            entity.ExpiresAtUtc,
            rawKey);
    }

    public async Task<IReadOnlyCollection<ApiKeyResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var keys = await dbContext.ApiKeys
            .OrderByDescending(k => k.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return keys.Select(k => new ApiKeyResponse(
            k.Id,
            k.Name,
            k.KeyPrefix,
            k.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries),
            k.IsActive,
            k.CreatedAtUtc,
            k.ExpiresAtUtc))
            .ToList();
    }

    public async Task RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        var key = await dbContext.ApiKeys.FindAsync([id], cancellationToken);
        if (key is null)
        {
            throw new NotFoundException($"API key {id} not found.");
        }

        key.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
