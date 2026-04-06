using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerApi.Contracts.Responses;
using CustomerApi.Domain.Models;
using CustomerApi.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomerApi.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptionsAccessor) : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptionsAccessor.Value;

    public AccessTokenResponse CreateToken(UserAccount userAccount)
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.TokenLifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userAccount.Email),
            new Claim(JwtRegisteredClaimNames.Email, userAccount.Email),
            new Claim(ClaimTypes.Email, userAccount.Email),
            new Claim(ClaimTypes.Name, userAccount.DisplayName),
            new Claim(ClaimTypes.Role, userAccount.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new AccessTokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            Role = userAccount.Role,
            Email = userAccount.Email
        };
    }
}
