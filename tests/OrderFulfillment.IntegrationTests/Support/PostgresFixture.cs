using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Infrastructure.Persistence;
using OrderFulfillment.Infrastructure.Queries;
using Testcontainers.PostgreSql;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Support;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17").Build();

    private NpgsqlDataSource? _dataSource;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _dataSource = NpgsqlDataSource.Create(_container.GetConnectionString());

        // Primenjujemo prave migracije, iste koje će se primeniti i u produkciji.
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dataSource is not null)
        {
            await _dataSource.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    public OrderFulfillmentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderFulfillmentDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new OrderFulfillmentDbContext(options);
    }

    public OrderReadService CreateReadService() =>
        new(_dataSource ?? throw new InvalidOperationException("Fixture nije inicijalizovan."));

    public async Task SaveOrderAsync(Order order)
    {
        await using var context = CreateDbContext();

        await new OrderRepository(context).AddAsync(order);
        await context.SaveChangesAsync();
    }

    public async Task<Order?> LoadOrderAsync(OrderId id)
    {
        await using var context = CreateDbContext();

        return await new OrderRepository(context).GetByIdAsync(id);
    }
}