using System.ComponentModel.DataAnnotations;

namespace CustomerApi.Contracts.Requests;

public sealed class CreateCustomerRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(32, MinimumLength = 7)]
    public string PhoneNumber { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}
