using CustomerApi.Controllers;
using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Exceptions;
using CustomerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task CreateTokenAsync_ReturnsOkWithAccessToken()
    {
        var controller = new AuthController(new FakeAuthService());

        var result = await controller.CreateTokenAsync(
            new LoginRequest
            {
                Email = "admin@customerapi.local",
                Password = "Admin123!"
            },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var token = Assert.IsType<AccessTokenResponse>(okResult.Value);
        Assert.Equal("Admin", token.Role);
    }

    [Fact]
    public async Task CreateTokenAsync_PropagatesAuthenticationFailuresToPipeline()
    {
        var controller = new AuthController(new FailingAuthService());

        await Assert.ThrowsAsync<AuthenticationException>(() => controller.CreateTokenAsync(
            new LoginRequest
            {
                Email = "bad@customerapi.local",
                Password = "WrongPassword!"
            },
            CancellationToken.None));
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Task<AccessTokenResponse> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new AccessTokenResponse
            {
                AccessToken = "fake-jwt",
                Email = request.Email,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1),
                Role = "Admin"
            });
    }

    private sealed class FailingAuthService : IAuthService
    {
        public Task<AccessTokenResponse> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken) =>
            throw new AuthenticationException("Invalid email or password.");
    }
}
