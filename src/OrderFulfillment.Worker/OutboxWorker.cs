using System.Diagnostics.CodeAnalysis;
using OrderFulfillment.Infrastructure.Persistence.Outbox;

namespace OrderFulfillment.Worker;

public sealed class OutboxWorker : BackgroundService
{
    private static readonly Action<ILogger, Exception?> LogStarted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogStarted)),
            "Outbox worker je pokrenut");

    private static readonly Action<ILogger, Exception?> LogRunFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(LogRunFailed)),
            "Obrada Outbox-a nije uspela, pokušaj ponovo posle pauze");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(
        IServiceScopeFactory scopeFactory,
        OutboxProcessorOptions options,
        ILogger<OutboxWorker> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.PollingInterval, TimeSpan.Zero);

        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(_logger, null);

        using var timer = new PeriodicTimer(_options.PollingInterval);

        try
        {
            do
            {
                await RunOnceSafelyAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Privremena greška (baza, broker) ne sme da obori proces, pokušaj se ponavlja.")]
    private async Task RunOnceSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            int processed;

            do
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                processed = await processor.ProcessBatchAsync(cancellationToken);
            }
            while (processed >= _options.BatchSize && !cancellationToken.IsCancellationRequested);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogRunFailed(_logger, exception);
        }
    }
}