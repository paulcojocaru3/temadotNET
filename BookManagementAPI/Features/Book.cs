using System.ComponentModel.DataAnnotations;
namespace BookManagementAPI.Features;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime PublishedDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}