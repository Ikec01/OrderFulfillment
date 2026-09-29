namespace OrderFulfillment.Worker;

public sealed class Worker : BackgroundService
{
    private static readonly Action<ILogger, DateTimeOffset, Exception?> LogWorkerRunning =
        LoggerMessage.Define<DateTimeOffset>(
            LogLevel.Information,
            new EventId(1, nameof(LogWorkerRunning)),
            "Worker running at: {Time}");

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogWorkerRunning(_logger, DateTimeOffset.UtcNow, null);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}