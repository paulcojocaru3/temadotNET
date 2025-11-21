using FluentValidation;
using BookManagementAPI.DTOs;
using BookManagementAPI.Features;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BookManagementAPI.Validators;

public class CreateBookProfileValidator : AbstractValidator<CreateBookProfileRequest>
{
    private readonly ApplicationContext _context;
    private readonly ILogger<CreateBookProfileValidator> _logger;

    private readonly string[] _inappropriateWords = { "badword", "offensive", "banned" };
    private readonly string[] _restrictedChildrenWords = { "violence", "horror", "adult", "death", "kill", "blood" };
    private readonly string[] _technicalKeywords = { "c#", ".net", "guide", "pattern", "coding", "architecture", "azure", "cloud" };    
    public CreateBookProfileValidator(ApplicationContext context, ILogger<CreateBookProfileValidator> logger)
    {
        _context = context;
        _logger = logger;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(1, 200).WithMessage("Title must be between 1 and 200 characters.")
            .Must(BeValidTitle).WithMessage("Title contains inappropriate content.")
            .MustAsync(BeUniqueTitle).WithMessage("A book with this title already exists for this author.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author name is required.")
            .Length(2, 100).WithMessage("Author name must be between 2 and 100 characters.")
            .Must(BeValidAuthorName).WithMessage("Author name contains invalid characters.");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Must(BeValidISBN).WithMessage("Invalid ISBN format.")
            .MustAsync(BeUniqueISBN).WithMessage("ISBN already exists in the system.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid book category.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThan(10000).WithMessage("Price cannot exceed $10,000.");

        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.")
            .Must(date => date.Year >= 1400).WithMessage("Published date cannot be before year 1400.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity exceeds reasonable limit (100,000).");

        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidImageUrl).When(x => !string.IsNullOrEmpty(x.CoverImageUrl))
            .WithMessage("Invalid Cover Image URL.");

        RuleFor(x => x)
            .MustAsync(PassBusinessRules).WithMessage("Business validation rules failed.");
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(20.00m)
            .When(x => x.Category == BookCategory.Technical)
            .WithMessage("Technical books must be at least $20.00.");

        RuleFor(x => x.PublishedDate)
            .Must(date => date >= DateTime.UtcNow.AddYears(-5))
            .When(x => x.Category == BookCategory.Technical)
            .WithMessage("Technical books must be published within the last 5 years.");


        RuleFor(x => x.Price)
            .LessThanOrEqualTo(50.00m)
            .When(x => x.Category == BookCategory.Children)
            .WithMessage("Children's books cannot exceed $50.00.");

        RuleFor(x => x.Title)
            .Must(BeAppropriateForChildren)
            .When(x => x.Category == BookCategory.Children)
            .WithMessage("Children's book title contains restricted words.");

        RuleFor(x => x.Author)
            .MinimumLength(5)
            .When(x => x.Category == BookCategory.Fiction)
            .WithMessage("Fiction authors must provide full name (minimum 5 characters).");


        RuleFor(x => x.StockQuantity)
            .LessThanOrEqualTo(20)
            .When(x => x.Price > 100)
            .WithMessage("High-value books (> $100) are limited to 20 stock units.");


        RuleFor(x => x)
            .MustAsync(CheckDailyLimit)
            .WithMessage("Daily book addition limit (500) reached.");
    }
    private bool BeValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        return !_inappropriateWords.Any(w => title.Contains(w, StringComparison.OrdinalIgnoreCase));
    }
    private async Task<bool> CheckDailyLimit(CreateBookProfileRequest model, CancellationToken token)
    {
        var today = DateTime.UtcNow.Date;
        var count = await _context.Books.CountAsync(b => b.CreatedAt >= today, token);
        return count < 500;
    }
    private bool BeAppropriateForChildren(string title)
    {
        if (string.IsNullOrEmpty(title)) return false;
        return !_restrictedChildrenWords.Any(w => title.Contains(w, StringComparison.OrdinalIgnoreCase));
    }
    private async Task<bool> BeUniqueTitle(CreateBookProfileRequest model, string title, CancellationToken token)
    {
        _logger.LogInformation("Validating uniqueness for Title: '{Title}' and Author: '{Author}'", title, model.Author);
        return !await _context.Books.AnyAsync(b => b.Title == title && b.Author == model.Author, token);
    }

    private bool BeValidAuthorName(string author)
    {
        return Regex.IsMatch(author, @"^[a-zA-Z\s\-\'\.]+$");
    }

    private bool BeValidISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return false;
        var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");
        return (cleanIsbn.Length == 10 || cleanIsbn.Length == 13) && long.TryParse(cleanIsbn, out _);
    }

    private async Task<bool> BeUniqueISBN(string isbn, CancellationToken token)
    {
        _logger.LogInformation("Validating ISBN uniqueness: {ISBN}", isbn);
        return !await _context.Books.AnyAsync(b => b.ISBN == isbn, token);
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult)) return false;
        if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps) return false;

        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return validExtensions.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> PassBusinessRules(CreateBookProfileRequest model, CancellationToken token)
    {
        _logger.LogInformation("Starting complex business rules validation for book: {Title}", model.Title);

        var today = DateTime.UtcNow.Date;
        var booksAddedToday = await _context.Books.CountAsync(b => b.CreatedAt >= today, token);
        if (booksAddedToday >= 500)
        {
            _logger.LogWarning("Daily book limit reached. Current count: {Count}", booksAddedToday);
            return false; 
        }

        if (model.Category == BookCategory.Technical && model.Price < 20.00m)
        {
            _logger.LogWarning("Validation Failed: Technical books must cost at least $20.00. Given: {Price}", model.Price);
            return false;
        }

        if (model.Category == BookCategory.Children)
        {
            if (_restrictedChildrenWords.Any(w => model.Title.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Validation Failed: Children's book title contains restricted content.");
                return false;
            }
        }

        if (model.Price > 500 && model.StockQuantity > 10)
        {
            _logger.LogWarning("Validation Failed: High-value book (> $500) stock limited to 10. Given: {Stock}", model.StockQuantity);
            return false;
        }

        return true;
    }
}