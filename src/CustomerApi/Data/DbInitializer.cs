using CustomerApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(CustomerDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Customers.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Customers.AddRange(
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
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
