using CustomerApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var customer = modelBuilder.Entity<Customer>();
        customer.ToTable("Customers");
        customer.HasKey(entity => entity.Id);
        customer.Property(entity => entity.FirstName).HasMaxLength(100).IsRequired();
        customer.Property(entity => entity.LastName).HasMaxLength(100).IsRequired();
        customer.Property(entity => entity.Email).HasMaxLength(256).IsRequired();
        customer.Property(entity => entity.PhoneNumber).HasMaxLength(32).IsRequired();
        customer.Property(entity => entity.IsActive).IsRequired();
        customer.Property(entity => entity.CreatedAtUtc).IsRequired();
        customer.Property(entity => entity.UpdatedAtUtc).IsRequired();
        customer.HasIndex(entity => entity.Email).IsUnique();
    }
}
