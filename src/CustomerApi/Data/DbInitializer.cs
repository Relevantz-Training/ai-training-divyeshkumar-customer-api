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
            },
            new Customer
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                FirstName = "Liam",
                LastName = "Johnson",
                Email = "liam.johnson@example.com",
                PhoneNumber = "+1-555-100-2004",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-45),
                UpdatedAtUtc = now.AddDays(-3)
            },
            new Customer
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                FirstName = "Sophia",
                LastName = "Martinez",
                Email = "sophia.martinez@example.com",
                PhoneNumber = "+1-555-100-2005",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-60),
                UpdatedAtUtc = now.AddDays(-10)
            },
            new Customer
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                FirstName = "Oliver",
                LastName = "Thompson",
                Email = "oliver.thompson@example.com",
                PhoneNumber = "+1-555-100-2006",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-7),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new Customer
            {
                Id = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123"),
                FirstName = "Emma",
                LastName = "Williams",
                Email = "emma.williams@example.com",
                PhoneNumber = "+1-555-100-2007",
                IsActive = false,
                CreatedAtUtc = now.AddDays(-90),
                UpdatedAtUtc = now.AddDays(-20)
            },
            new Customer
            {
                Id = Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234"),
                FirstName = "James",
                LastName = "Anderson",
                Email = "james.anderson@example.com",
                PhoneNumber = "+1-555-100-2008",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-25),
                UpdatedAtUtc = now.AddDays(-4)
            },
            new Customer
            {
                Id = Guid.Parse("f6a7b8c9-d0e1-2345-fabc-456789012345"),
                FirstName = "Isabella",
                LastName = "Garcia",
                Email = "isabella.garcia@example.com",
                PhoneNumber = "+1-555-100-2009",
                IsActive = true,
                CreatedAtUtc = now.AddDays(-12),
                UpdatedAtUtc = now.AddDays(-2)
            },
            new Customer
            {
                Id = Guid.Parse("a7b8c9d0-e1f2-3456-abcd-567890123456"),
                FirstName = "Ethan",
                LastName = "Lee",
                Email = "ethan.lee@example.com",
                PhoneNumber = "+1-555-100-2010",
                IsActive = false,
                CreatedAtUtc = now.AddDays(-50),
                UpdatedAtUtc = now.AddDays(-8)
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
