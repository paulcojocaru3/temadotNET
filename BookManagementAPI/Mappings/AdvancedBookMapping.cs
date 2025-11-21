using AutoMapper;
using AutoMapper.Features;
using BookManagementAPI.DTOs;
using BookManagementAPI.Features;

namespace BookManagementAPI.Mappings;

public class AdvancedBookMappingProfile : Profile
{
    public AdvancedBookMappingProfile()
    {

        CreateMap<CreateBookProfileRequest, Book>()
            .ConstructUsing(src => new Book
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null, 
                IsAvailable = src.StockQuantity > 0,
                Title = src.Title,
                Author = src.Author,
                ISBN = src.ISBN,
                Category = src.Category,
                Price = src.Price,
                PublishedDate = src.PublishedDate,
                CoverImageUrl = src.CoverImageUrl,
                StockQuantity = src.StockQuantity
            });


        CreateMap<Book, BookProfileDto>()
            .ConstructUsing(src => new BookProfileDto
            {
                Id = src.Id,
                Title = src.Title,
                Author = src.Author,
                ISBN = src.ISBN,
                PublishedDate = src.PublishedDate,
                CreatedAt = src.CreatedAt,
                StockQuantity = src.StockQuantity,
                IsAvailable = src.IsAvailable,
                CategoryDisplayName = ResolveCategoryName(src.Category),
                Price = CalculateEffectivePrice(src),
                FormattedPrice = CalculateEffectivePrice(src).ToString("C2"),
                CoverImageUrl = src.Category == BookCategory.Children ? null : src.CoverImageUrl,
                PublishedAge = CalculateBookAge(src.PublishedDate),
                AuthorInitials = GetAuthorInitials(src.Author),
                AvailabilityStatus = GetAvailabilityStatus(src)
            });
    }


    private static string ResolveCategoryName(BookCategory category) => category switch
    {
        BookCategory.Fiction => "Fiction & Literature",
        BookCategory.NonFiction => "Non-Fiction",
        BookCategory.Technical => "Technical & Professional",
        BookCategory.Children => "Children's Books",
        _ => "Uncategorized"
    };

    private static decimal CalculateEffectivePrice(Book book)
    {
        return book.Category == BookCategory.Children
            ? book.Price * 0.9m
            : book.Price;
    }

    private static string CalculateBookAge(DateTime publishedDate)
    {
        var days = (DateTime.UtcNow - publishedDate).TotalDays;
        return days switch
        {
            < 30 => "New Release",
            < 365 => $"{Math.Floor(days / 30)} months old",
            < 1825 => $"{Math.Floor(days / 365)} years old",
            _ => "Classic"
        };
    }

    private static string GetAuthorInitials(string author)
    {
        if (string.IsNullOrWhiteSpace(author)) return "?";
        var parts = author.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length >= 2)
            return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
            
        return char.ToUpper(parts[0][0]).ToString();
    }

    private static string GetAvailabilityStatus(Book book)
    {
        if (!book.IsAvailable) return "Out of Stock";
        if (book.StockQuantity == 0) return "Unavailable";
        if (book.StockQuantity == 1) return "Last Copy";
        if (book.StockQuantity <= 5) return "Limited Stock";
        return "In Stock";
    }
}