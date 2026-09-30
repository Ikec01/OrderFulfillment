using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderFulfillment.Application.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private static readonly Action<ILogger, string, Exception?> LogHandling =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(LogHandling)),
                "Handling {RequestName}");

        private static readonly Action<ILogger, string, long, Exception?> LogHandled =
            LoggerMessage.Define<string, long>(
                LogLevel.Information,
                new EventId(2, nameof(LogHandled)),
                "Handled {RequestName} in {ElapsedMilliseconds} ms");
        private static readonly Action<ILogger, string, long, Exception?> LogFailed =
        LoggerMessage.Define<string, long>(
            LogLevel.Warning,
            new EventId(3, nameof(LogFailed)),
            "Request {RequestName} failed after {ElapsedMilliseconds} ms");
        public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next);

            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            LogHandling(logger, requestName, null);

            try
            {
                var response = await next(cancellationToken);

                LogHandled(logger, requestName, stopwatch.ElapsedMilliseconds, null);

                return response;
            }
            catch (Exception exception)
            {
                LogFailed(logger, requestName, stopwatch.ElapsedMilliseconds, exception);

                throw;
            }
        }

    }
}
