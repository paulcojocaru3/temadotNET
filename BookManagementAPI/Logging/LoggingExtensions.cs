using Microsoft.Extensions.Logging;

namespace BookManagementAPI.Logging;

public class LoggingExtensions
{
    private static readonly Action<ILogger, BookCreationMetrics, Exception?> _logBookMetrics =
        LoggerMessage.Define<BookCreationMetrics>(
            LogLevel.Information,
            new EventId(Logging.BookCreationCompleted, nameof(Logging.BookCreationCompleted)),
            "Book Operation Metrics: {@Metrics}" 
        );

    public static void LogBookCreationMetrics(ILogger logger, BookCreationMetrics metrics)
    {
        _logBookMetrics(logger, metrics, null);
    }
}