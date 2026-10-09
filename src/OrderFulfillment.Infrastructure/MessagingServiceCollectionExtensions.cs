using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using OrderFulfillment.Infrastructure.Messaging;

namespace OrderFulfillment.Infrastructure;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMessaging(
        this IServiceCollection services,
        RabbitMqOptions options,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Username) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "RabbitMq:Username i RabbitMq:Password moraju biti postavljeni (user-secrets ili promenljive okruženja).");
        }

        services.AddMassTransit(configurator =>
        {
            configure?.Invoke(configurator);

            configurator.UsingRabbitMq((context, bus) =>
            {
                bus.Host(options.Host, options.Port, options.VirtualHost, host =>
                {
                    host.Username(options.Username);
                    host.Password(options.Password);
                });

                bus.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

        return services;
    }
}