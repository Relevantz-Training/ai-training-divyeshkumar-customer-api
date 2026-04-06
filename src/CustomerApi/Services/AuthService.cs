using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Domain.Models;
using CustomerApi.Exceptions;

namespace CustomerApi.Services;

public sealed class AuthService(ITokenService tokenService) : IAuthService
{
    private static readonly IReadOnlyCollection<UserAccount> Users =
    [
        new UserAccount
        {
            Email = "admin@customerapi.local",
            Password = "Admin123!",
            Role = "Admin",
            DisplayName = "Customer API Administrator"
        },
        new UserAccount
        {
            Email = "support@customerapi.local",
            Password = "Support123!",
            Role = "Support",
            DisplayName = "Customer API Support"
        }
    ];

    public Task<AccessTokenResponse> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = Users.FirstOrDefault(existingUser =>
            string.Equals(existingUser.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null || !string.Equals(user.Password, request.Password, StringComparison.Ordinal))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        return Task.FromResult(tokenService.CreateToken(user));
    }
}
