using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Domain.Models;
using CustomerApi.Exceptions;
using CustomerApi.Repositories;

namespace CustomerApi.Services;

public sealed class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    public async Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(MapToResponse).ToArray();
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null
            ? throw new NotFoundException($"Customer '{id}' was not found.")
            : MapToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingCustomer = await customerRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingCustomer is not null)
        {
            throw new ConflictException($"A customer with email '{normalizedEmail}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber.Trim(),
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var createdCustomer = await customerRepository.AddAsync(customer, cancellationToken);
        return MapToResponse(createdCustomer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var existingCustomer = await customerRepository.GetByIdAsync(id, cancellationToken);
        if (existingCustomer is null)
        {
            throw new NotFoundException($"Customer '{id}' was not found.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var customerByEmail = await customerRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (customerByEmail is not null && customerByEmail.Id != id)
        {
            throw new ConflictException($"A customer with email '{normalizedEmail}' already exists.");
        }

        existingCustomer.FirstName = request.FirstName.Trim();
        existingCustomer.LastName = request.LastName.Trim();
        existingCustomer.Email = normalizedEmail;
        existingCustomer.PhoneNumber = request.PhoneNumber.Trim();
        existingCustomer.IsActive = request.IsActive;
        existingCustomer.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var updatedCustomer = await customerRepository.UpdateAsync(existingCustomer, cancellationToken);
        return MapToResponse(updatedCustomer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existingCustomer = await customerRepository.GetByIdAsync(id, cancellationToken);
        if (existingCustomer is null)
        {
            throw new NotFoundException($"Customer '{id}' was not found.");
        }

        await customerRepository.DeleteAsync(id, cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static CustomerResponse MapToResponse(Customer customer) =>
        new()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            IsActive = customer.IsActive,
            CreatedAtUtc = customer.CreatedAtUtc,
            UpdatedAtUtc = customer.UpdatedAtUtc
        };
}
