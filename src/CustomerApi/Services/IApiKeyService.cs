using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;

namespace CustomerApi.Services;

public interface IApiKeyService
{
    Task<CreateApiKeyResponse> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ApiKeyResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task RevokeAsync(Guid id, CancellationToken cancellationToken);
}
