using System.Collections.Concurrent;
using OrderFulfillment.Infrastructure.Messaging;

namespace OrderFulfillment.IntegrationTests.Support;

internal sealed record PublishedMessage(Guid MessageId, object Event);

internal sealed class FakeIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ConcurrentQueue<PublishedMessage> _published = new();

    private int _attemptCount;

    public IReadOnlyList<PublishedMessage> Published => _published.ToArray();

    public int AttemptCount => Volatile.Read(ref _attemptCount);

    public bool ShouldFail { get; set; }

    public TimeSpan Delay { get; set; }

    public async Task PublishAsync(
        Guid messageId,
        object integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _attemptCount);

        if (Delay > TimeSpan.Zero)
        {
            await Task.Delay(Delay, cancellationToken);
        }

        if (ShouldFail)
        {
            throw new InvalidOperationException("Broker nije dostupan.");
        }

        _published.Enqueue(new PublishedMessage(messageId, integrationEvent));
    }
}