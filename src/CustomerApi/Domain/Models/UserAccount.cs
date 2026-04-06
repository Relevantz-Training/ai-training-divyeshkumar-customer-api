namespace CustomerApi.Domain.Models;

public sealed class UserAccount
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
}
