using Microsoft.Extensions.DependencyInjection;
using OrderFulfillment.Infrastructure.Persistence.Outbox;

namespace OrderFulfillment.Infrastructure;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxProcessing(
        this IServiceCollection services,
        OutboxProcessorOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(options ?? new OutboxProcessorOptions());
        services.AddScoped<OutboxProcessor>();

        return services;
    }
}