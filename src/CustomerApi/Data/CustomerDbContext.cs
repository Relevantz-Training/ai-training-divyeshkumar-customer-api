using CustomerApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

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

        var apiKey = modelBuilder.Entity<ApiKey>();
        apiKey.ToTable("ApiKeys");
        apiKey.HasKey(entity => entity.Id);
        apiKey.Property(entity => entity.Name).HasMaxLength(100).IsRequired();
        apiKey.Property(entity => entity.KeyPrefix).HasMaxLength(8).IsRequired();
        apiKey.Property(entity => entity.KeyHash).HasMaxLength(64).IsRequired();
        apiKey.Property(entity => entity.Roles).HasMaxLength(256).IsRequired();
        apiKey.Property(entity => entity.IsActive).IsRequired();
        apiKey.Property(entity => entity.CreatedAtUtc).IsRequired();
        apiKey.HasIndex(entity => entity.KeyPrefix);
    }
}
