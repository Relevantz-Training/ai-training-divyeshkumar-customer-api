using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Issues a JWT for one of the mock local users.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> CreateTokenAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var accessToken = await authService.AuthenticateAsync(request, cancellationToken);
        return Ok(accessToken);
    }
}
