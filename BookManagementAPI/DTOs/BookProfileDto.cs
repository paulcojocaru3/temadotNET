
namespace BookManagementAPI.DTOs;
public record BookProfileDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string ISBN { get; init; } = string.Empty;
    public string CategoryDisplayName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string FormattedPrice { get; init; } = string.Empty;
    public DateTime PublishedDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CoverImageUrl { get; init; }
    public bool IsAvailable { get; init; }
    public int StockQuantity { get; init; }
    public string PublishedAge { get; init; } = string.Empty;
    public string AuthorInitials { get; init; } = string.Empty;
    public string AvailabilityStatus { get; init; } = string.Empty;
}