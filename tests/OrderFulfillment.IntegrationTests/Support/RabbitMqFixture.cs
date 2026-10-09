using OrderFulfillment.Infrastructure.Messaging;
using Testcontainers.RabbitMq;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Support;

public sealed class RabbitMqFixture : IAsyncLifetime
{
    private const string TestUser = "test";
    private const string TestPassword = "test";

    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:3.13-management")
        .WithUsername(TestUser)
        .WithPassword(TestPassword)
        .Build();

    public RabbitMqOptions Options => new()
    {
        Host = _container.Hostname,
        Port = _container.GetMappedPublicPort(5672),
        VirtualHost = "/",
        Username = TestUser,
        Password = TestPassword,
    };

    public Task InitializeAsync() => _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}