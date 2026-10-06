using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Infrastructure.Persistence;

namespace OrderFulfillment.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<OrderFulfillmentDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrderFulfillmentDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
