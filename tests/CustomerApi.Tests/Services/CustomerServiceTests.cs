using CustomerApi.Contracts.Requests;
using CustomerApi.Domain.Models;
using CustomerApi.Exceptions;
using CustomerApi.Repositories;
using CustomerApi.Services;

namespace CustomerApi.Tests.Services;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSeededCustomers()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        var customers = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, customers.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenCustomerMissing()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictForDuplicateEmail()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        var request = new CreateCustomerRequest
        {
            FirstName = "Ava",
            LastName = "Patel",
            Email = "ava.patel@example.com",
            PhoneNumber = "+1-555-100-2001",
            IsActive = true
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_AddsCustomerWithNormalizedEmail()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        var response = await service.CreateAsync(new CreateCustomerRequest
        {
            FirstName = "Taylor",
            LastName = "Morgan",
            Email = "  TAYLOR.MORGAN@EXAMPLE.COM ",
            PhoneNumber = "+1-555-222-3333",
            IsActive = true
        }, CancellationToken.None);

        Assert.Equal("taylor.morgan@example.com", response.Email);
        Assert.Equal(3, repository.Customers.Count);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflictWhenEmailBelongsToAnotherCustomer()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(
            FakeCustomerRepository.CustomerOne.Id,
            new UpdateCustomerRequest
            {
                FirstName = "Ava",
                LastName = "Patel",
                Email = FakeCustomerRepository.CustomerTwo.Email,
                PhoneNumber = "+1-555-100-2020",
                IsActive = true
            },
            CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingCustomer()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        var response = await service.UpdateAsync(
            FakeCustomerRepository.CustomerOne.Id,
            new UpdateCustomerRequest
            {
                FirstName = "Ava",
                LastName = "Patel-Updated",
                Email = "ava.updated@example.com",
                PhoneNumber = "+1-555-000-0000",
                IsActive = false
            },
            CancellationToken.None);

        Assert.Equal("Patel-Updated", response.LastName);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCustomer()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        await service.DeleteAsync(FakeCustomerRepository.CustomerOne.Id, CancellationToken.None);

        Assert.Single(repository.Customers);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenCustomerMissing()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        internal static readonly Customer CustomerOne = new()
        {
            Id = Guid.Parse("f597147d-5b06-460a-a65e-7e5fa1c4da58"),
            FirstName = "Ava",
            LastName = "Patel",
            Email = "ava.patel@example.com",
            PhoneNumber = "+1-555-100-2001",
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        internal static readonly Customer CustomerTwo = new()
        {
            Id = Guid.Parse("7bd37b51-4da2-4b0a-b8ae-11d8be747a61"),
            FirstName = "Noah",
            LastName = "Rivera",
            Email = "noah.rivera@example.com",
            PhoneNumber = "+1-555-100-2002",
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-8),
            UpdatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        internal List<Customer> Customers { get; } = [Clone(CustomerOne), Clone(CustomerTwo)];

        public Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Customer>>(Customers.Select(Clone).ToArray());

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Customers.FirstOrDefault(customer => customer.Id == id) is { } customer ? Clone(customer) : null);

        public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(Customers.FirstOrDefault(customer =>
                string.Equals(customer.Email, email, StringComparison.OrdinalIgnoreCase)) is { } customer ? Clone(customer) : null);

        public Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            var clone = Clone(customer);
            Customers.Add(clone);
            return Task.FromResult(Clone(clone));
        }

        public Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken)
        {
            var index = Customers.FindIndex(existingCustomer => existingCustomer.Id == customer.Id);
            Customers[index] = Clone(customer);
            return Task.FromResult(Clone(Customers[index]));
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            Customers.RemoveAll(customer => customer.Id == id);
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
}
