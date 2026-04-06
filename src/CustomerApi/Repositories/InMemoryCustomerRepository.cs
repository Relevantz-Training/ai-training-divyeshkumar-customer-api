using System.Collections.Concurrent;
using CustomerApi.Domain.Models;

namespace CustomerApi.Repositories;

public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly ConcurrentDictionary<Guid, Customer> _customers;

    public InMemoryCustomerRepository()
    {
        var now = DateTimeOffset.UtcNow;
        var seededCustomers = new[]
        {
            new Customer
            {
                Id = Guid.Parse("f597147d-5b06-460a-a65e-7e5fa1c4da58"),
                FirstName = "Ava",
                LastName = "Patel",
                Email = "ava.patel@example.com",
                PhoneNumber = "+1-555-100-2001",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-20),
                UpdatedAtUtc = now.AddDays(-2)
            },
            new Customer
            {
                Id = Guid.Parse("7bd37b51-4da2-4b0a-b8ae-11d8be747a61"),
                FirstName = "Noah",
                LastName = "Rivera",
                Email = "noah.rivera@example.com",
                PhoneNumber = "+1-555-100-2002",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-14),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new Customer
            {
                Id = Guid.Parse("642165e7-7cf2-4fbb-9402-3ef1e00bc37d"),
                FirstName = "Mia",
                LastName = "Chen",
                Email = "mia.chen@example.com",
                PhoneNumber = "+1-555-100-2003",
                IsActive = false,
                CreatedAtUtc = now.AddDays(-30),
                UpdatedAtUtc = now.AddDays(-5)
            }
        };

        _customers = new ConcurrentDictionary<Guid, Customer>(seededCustomers.ToDictionary(customer => customer.Id, Clone));
    }

    public Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var customers = _customers.Values
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .Select(Clone)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<Customer>>(customers);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _customers.TryGetValue(id, out var customer);
        return Task.FromResult(customer is null ? null : Clone(customer));
    }

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var customer = _customers.Values.FirstOrDefault(existingCustomer =>
            string.Equals(existingCustomer.Email.ToUpperInvariant(), normalizedEmail, StringComparison.Ordinal));

        return Task.FromResult(customer is null ? null : Clone(customer));
    }

    public Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var storedCustomer = Clone(customer);
        _customers[storedCustomer.Id] = storedCustomer;
        return Task.FromResult(Clone(storedCustomer));
    }

    public Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var storedCustomer = Clone(customer);
        _customers[storedCustomer.Id] = storedCustomer;
        return Task.FromResult(Clone(storedCustomer));
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _customers.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private static Customer Clone(Customer customer) =>
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
