using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFulfillment.Infrastructure.Persistence;

public sealed class OrderFulfillmentDbContextFactory : IDesignTimeDbContextFactory<OrderFulfillmentDbContext>
{
    public const string ConnectionStringVariable = "ORDERFULFILLMENT_DB";

    public OrderFulfillmentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Postavi promenljivu okruženja {ConnectionStringVariable} sa connection string-om.");
        }

        var options = new DbContextOptionsBuilder<OrderFulfillmentDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OrderFulfillmentDbContext(options);
    }
}
