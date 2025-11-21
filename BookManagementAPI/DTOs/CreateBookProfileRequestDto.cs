using BookManagementAPI.Features;

namespace BookManagementAPI.DTOs;

public record CreateBookProfileRequest(
    string Title,
    string Author,
    string ISBN,
    BookCategory Category,
    decimal Price,
    DateTime PublishedDate,
    string? CoverImageUrl,
    int StockQuantity = 1
    );