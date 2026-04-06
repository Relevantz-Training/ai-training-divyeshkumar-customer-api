using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;

namespace CustomerApi.Services;

public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);

    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
