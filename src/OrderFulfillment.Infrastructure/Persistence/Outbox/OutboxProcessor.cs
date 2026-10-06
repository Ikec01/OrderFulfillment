using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFulfillment.Infrastructure.Messaging;

namespace OrderFulfillment.Infrastructure.Persistence.Outbox;

public sealed class OutboxProcessor
{
    private static readonly Action<ILogger, int, Exception?> LogBatchProcessed =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1, nameof(LogBatchProcessed)),
            "Outbox: objavljeno {Count} poruka");

    private static readonly Action<ILogger, Guid, string, int, Exception?> LogPublishFailed =
        LoggerMessage.Define<Guid, string, int>(
            LogLevel.Warning,
            new EventId(2, nameof(LogPublishFailed)),
            "Outbox: objavljivanje poruke {MessageId} ({MessageType}) nije uspelo, pokušaj {Attempts}");

    private static readonly Action<ILogger, Guid, string, int, Exception?> LogMessageAbandoned =
        LoggerMessage.Define<Guid, string, int>(
            LogLevel.Error,
            new EventId(3, nameof(LogMessageAbandoned)),
            "Outbox: poruka {MessageId} ({MessageType}) odbačena posle {Attempts} pokušaja, potrebna je ručna provera");

    private readonly OrderFulfillmentDbContext _dbContext;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly TimeProvider _timeProvider;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        OrderFulfillmentDbContext dbContext,
        IIntegrationEventPublisher publisher,
        TimeProvider timeProvider,
        OutboxProcessorOptions options,
        ILogger<OutboxProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.BatchSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxAttempts, 1);

        _dbContext = dbContext;
        _publisher = publisher;
        _timeProvider = timeProvider;
        _options = options;
        _logger = logger;
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var batchSize = _options.BatchSize;
        var maxAttempts = _options.MaxAttempts;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var messages = await _dbContext.OutboxMessages
            .FromSql($"""
                SELECT * FROM outbox_messages
                WHERE "ProcessedOnUtc" IS NULL AND "Attempts" < {maxAttempts}
                ORDER BY "OccurredOnUtc", "Id"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return 0;
        }

        var processed = 0;

        foreach (var message in messages)
        {
            if (!await TryPublishAsync(message, cancellationToken))
            {
                break;
            }

            processed++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (processed > 0)
        {
            LogBatchProcessed(_logger, processed, null);
        }

        return processed;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Svaki neuspeh objavljivanja se beleži na poruci i ponavlja kasnije.")]
    private async Task<bool> TryPublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var integrationEvent = OutboxSerializer.Deserialize(message.Type, message.Content);

            await _publisher.PublishAsync(message.Id, integrationEvent, cancellationToken);

            message.MarkProcessed(_timeProvider.GetUtcNow().UtcDateTime);

            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            message.MarkFailed($"{exception.GetType().Name}: {exception.Message}");

            LogPublishFailed(_logger, message.Id, message.Type, message.Attempts, exception);

            if (message.Attempts >= _options.MaxAttempts)
            {
                LogMessageAbandoned(_logger, message.Id, message.Type, message.Attempts, null);
            }

            return false;
        }
    }
}