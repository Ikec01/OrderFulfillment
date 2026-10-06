using Microsoft.EntityFrameworkCore;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Infrastructure.Persistence
{
    public sealed class OrderFulfillmentDbContext(DbContextOptions<OrderFulfillmentDbContext> options)
        : DbContext(options), IUnitOfWork
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFulfillmentDbContext).Assembly);
        }
    }
}
