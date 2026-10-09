using OrderFulfillment.Infrastructure;
using OrderFulfillment.Infrastructure.Messaging;
using OrderFulfillment.Infrastructure.Persistence.Outbox;
using OrderFulfillment.Worker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Nedostaje connection string 'Postgres'.");

var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()
    ?? throw new InvalidOperationException("Nedostaje sekcija 'RabbitMq' u konfiguraciji.");

var outboxOptions = builder.Configuration.GetSection("Outbox").Get<OutboxProcessorOptions>()
    ?? new OutboxProcessorOptions();

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddOutboxProcessing(outboxOptions);
builder.Services.AddRabbitMessaging(rabbitMqOptions);
builder.Services.AddHostedService<OutboxWorker>();

await builder.Build().RunAsync();