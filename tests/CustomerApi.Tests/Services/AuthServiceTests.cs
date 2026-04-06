using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Domain.Models;
using CustomerApi.Exceptions;
using CustomerApi.Services;

namespace CustomerApi.Tests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ReturnsTokenForValidUser()
    {
        var tokenService = new FakeTokenService();
        var authService = new AuthService(tokenService);

        var response = await authService.AuthenticateAsync(new LoginRequest
        {
            Email = "admin@customerapi.local",
            Password = "Admin123!"
        }, CancellationToken.None);

        Assert.Equal("Admin", response.Role);
        Assert.Equal("admin@customerapi.local", response.Email);
    }

    [Fact]
    public async Task AuthenticateAsync_ThrowsForUnknownUser()
    {
        var authService = new AuthService(new FakeTokenService());

        await Assert.ThrowsAsync<AuthenticationException>(() => authService.AuthenticateAsync(new LoginRequest
        {
            Email = "unknown@customerapi.local",
            Password = "Admin123!"
        }, CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateAsync_ThrowsForBadPassword()
    {
        var authService = new AuthService(new FakeTokenService());

        await Assert.ThrowsAsync<AuthenticationException>(() => authService.AuthenticateAsync(new LoginRequest
        {
            Email = "support@customerapi.local",
            Password = "WrongPassword!"
        }, CancellationToken.None));
    }

    private sealed class FakeTokenService : ITokenService
    {
        public AccessTokenResponse CreateToken(UserAccount userAccount) =>
            new()
            {
                AccessToken = "fake-jwt-token",
                Email = userAccount.Email,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1),
                Role = userAccount.Role
            };
    }
}
