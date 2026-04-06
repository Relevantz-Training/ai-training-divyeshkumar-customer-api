using CustomerApi.Contracts.Responses;
using CustomerApi.Domain.Models;

namespace CustomerApi.Services;

public interface ITokenService
{
    AccessTokenResponse CreateToken(UserAccount userAccount);
}
