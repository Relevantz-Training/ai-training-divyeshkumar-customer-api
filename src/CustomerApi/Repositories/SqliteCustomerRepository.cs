using CustomerApi.Data;
using CustomerApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Repositories;

public sealed class SqliteCustomerRepository(CustomerDbContext dbContext) : ICustomerRepository
{
    public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(customer => customer.Email == normalizedEmail, cancellationToken);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken)
    {
        dbContext.Customers.Update(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (customer is null)
        {
            return;
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
