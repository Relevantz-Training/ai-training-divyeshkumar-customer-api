using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;

namespace CustomerApi.Services;

public interface IAuthService
{
    Task<AccessTokenResponse> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken);
}
