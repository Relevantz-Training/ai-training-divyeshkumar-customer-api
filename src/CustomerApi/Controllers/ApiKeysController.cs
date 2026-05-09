using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Controllers;

/// <summary>
/// Manages API keys for machine-to-machine access. Requires Admin JWT to manage keys.
/// </summary>
[ApiController]
[Route("api/apikeys")]
[Authorize(Policy = "CanManageApiKeys")]
public sealed class ApiKeysController(IApiKeyService apiKeyService) : ControllerBase
{
    /// <summary>
    /// Creates a new API key. Keys are read-only (Support role) and can only call GET /api/customers endpoints.
    /// The raw key is returned once and never stored — save it immediately.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateApiKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateApiKeyResponse>> CreateAsync(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiKeyService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Returns all API keys (prefix and metadata only — no raw keys or hashes).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ApiKeyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ApiKeyResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var keys = await apiKeyService.GetAllAsync(cancellationToken);
        return Ok(keys);
    }

    /// <summary>
    /// Revokes (deactivates) an API key. The key can no longer be used for authentication.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        await apiKeyService.RevokeAsync(id, cancellationToken);
        return NoContent();
    }
}
