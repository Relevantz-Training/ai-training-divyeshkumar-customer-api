using CustomerApi.Controllers;
using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using CustomerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Tests.Controllers;

public sealed class CustomersControllerTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOkWithCustomers()
    {
        var service = new FakeCustomerService();
        var controller = new CustomersController(service);

        var result = await controller.GetAllAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var customers = Assert.IsAssignableFrom<IReadOnlyCollection<CustomerResponse>>(okResult.Value);
        Assert.Single(customers);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOkWithCustomer()
    {
        var service = new FakeCustomerService();
        var controller = new CustomersController(service);

        var result = await controller.GetByIdAsync(FakeCustomerService.Customer.Id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var customer = Assert.IsType<CustomerResponse>(okResult.Value);
        Assert.Equal(FakeCustomerService.Customer.Id, customer.Id);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedAtAction()
    {
        var service = new FakeCustomerService();
        var controller = new CustomersController(service);

        var request = new CreateCustomerRequest
        {
            FirstName = "Taylor",
            LastName = "Morgan",
            Email = "taylor.morgan@example.com",
            PhoneNumber = "+1-555-222-3333",
            IsActive = true
        };

        var result = await controller.CreateAsync(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var customer = Assert.IsType<CustomerResponse>(createdResult.Value);
        Assert.Equal(nameof(CustomersController.GetByIdAsync), createdResult.ActionName);
        Assert.Equal(request.Email, customer.Email);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsOkWithUpdatedCustomer()
    {
        var service = new FakeCustomerService();
        var controller = new CustomersController(service);

        var request = new UpdateCustomerRequest
        {
            FirstName = "Ava",
            LastName = "Patel-Updated",
            Email = "ava.updated@example.com",
            PhoneNumber = "+1-555-100-2009",
            IsActive = false
        };

        var result = await controller.UpdateAsync(FakeCustomerService.Customer.Id, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var customer = Assert.IsType<CustomerResponse>(okResult.Value);
        Assert.Equal(request.LastName, customer.LastName);
        Assert.Equal(request.Email, customer.Email);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNoContent()
    {
        var service = new FakeCustomerService();
        var controller = new CustomersController(service);

        var result = await controller.DeleteAsync(FakeCustomerService.Customer.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    private sealed class FakeCustomerService : ICustomerService
    {
        internal static readonly CustomerResponse Customer = new()
        {
            Id = Guid.Parse("f597147d-5b06-460a-a65e-7e5fa1c4da58"),
            FirstName = "Ava",
            LastName = "Patel",
            Email = "ava.patel@example.com",
            PhoneNumber = "+1-555-100-2001",
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-5),
            UpdatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        public Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<CustomerResponse>>([Customer]);

        public Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Customer);

        public Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new CustomerResponse
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.IsActive,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });

        public Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new CustomerResponse
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.IsActive,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-7),
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
