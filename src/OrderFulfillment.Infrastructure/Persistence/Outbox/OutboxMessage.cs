using OrderFulfillment.Domain.Common;

namespace OrderFulfillment.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public const int MaxTypeLength = 300;
    public const int MaxErrorLength = 2000;

    // Potreban EF Core-u za materijalizaciju objekta iz baze.
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public int Attempts { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var integrationEvent = IntegrationEventMapper.Map(domainEvent);

        return new OutboxMessage
        {
            Id = domainEvent.EventId,
            Type = integrationEvent.GetType().FullName!,
            Content = OutboxSerializer.Serialize(integrationEvent),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        };
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Attempts++;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        Attempts++;
        Error = error.Length > MaxErrorLength ? error[..MaxErrorLength] : error;
    }
}