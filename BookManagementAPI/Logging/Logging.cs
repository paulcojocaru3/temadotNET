using BookManagementAPI.Features;

namespace BookManagementAPI.Logging;

public static class Logging
{
    public const int BookCreationStarted = 2001;
    public const int BookValidationFailed = 2002;
    public const int BookCreationCompleted = 2003;
    public const int DatabaseOperationStarted = 2004;
    public const int DatabaseOperationCompleted = 2005;
    public const int CacheOperationPerformed = 2006;
    public const int ISBNValidationPerformed = 2007;
    public const int StockValidationPerformed = 2008;
}

public record BookCreationMetrics(
    string OperationId,
    string BookTitle,
    string ISBN,
    BookCategory Category,
    double ValidationDurationMs,
    double DatabaseSaveDurationMs,
    double TotalDurationMs,
    bool Success,
    string? ErrorReason = null
    );