namespace OrderFulfillment.Infrastructure.Persistence.Outbox;

public sealed class OutboxProcessorOptions
{
    public int BatchSize { get; init; } = 20;

    public int MaxAttempts { get; init; } = 5;

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(2);
}