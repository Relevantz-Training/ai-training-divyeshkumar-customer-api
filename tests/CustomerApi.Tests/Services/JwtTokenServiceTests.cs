using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomerApi.Domain.Models;
using CustomerApi.Security;
using CustomerApi.Services;
using Microsoft.Extensions.Options;

namespace CustomerApi.Tests.Services;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_EmbedsExpectedClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "CustomerApi",
            Audience = "CustomerApiClients",
            SigningKey = "unit-test-signing-key-with-sufficient-length-12345",
            TokenLifetimeMinutes = 60
        });

        var service = new JwtTokenService(options);

        var response = service.CreateToken(new UserAccount
        {
            Email = "admin@customerapi.local",
            DisplayName = "Admin User",
            Password = "unused",
            Role = "Admin"
        });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == "admin@customerapi.local");
        Assert.Equal("Admin", response.Role);
    }
}
