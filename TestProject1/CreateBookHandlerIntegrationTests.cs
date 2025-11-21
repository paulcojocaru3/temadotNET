using AutoMapper;
using BookManagementAPI.DTOs;
using BookManagementAPI.Features;
using BookManagementAPI.Mappings;
using BookManagementAPI.Logging; // Pentru constantele LogEvents
using Data;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace BookManagementAPI.TestProject1;

public class CreateBookHandlerIntegrationTests : IDisposable
{
    private readonly ApplicationContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateBookHandler>> _loggerMock;
    private readonly CreateBookHandler _handler;

    public CreateBookHandlerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationContext(options);

        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(AdvancedBookMappingProfile).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<CreateBookHandler>>();
        _handler = new CreateBookHandler(_context, _mapper, _cache, _loggerMock.Object);
    }


    [Fact]
    public async Task Handle_ValidTechnicalBookRequest_CreatesBookWithCorrectMappings()
    {

        var request = new CreateBookProfileRequest(
            Title: "Book Testnr1",
            Author: "Paul C",
            ISBN: "978-3-16-148410-0",
            Category: BookCategory.Technical,
            Price: 49.99m,
            PublishedDate: DateTime.UtcNow.AddDays(-10),
            CoverImageUrl: "http://site.com/img.jpg",
            StockQuantity: 3
        );


        var result = await _handler.Handle(new CreateBookCommand(request), CancellationToken.None);

  
        result.Should().NotBeNull();
        result.CategoryDisplayName.Should().Be("Technical & Professional"); 
        result.AuthorInitials.Should().Be("JD"); 
        result.PublishedAge.Should().Be("New Release"); 
        result.FormattedPrice.Should().StartWith("$"); 
        result.AvailabilityStatus.Should().Be("Limited Stock");
        result.CoverImageUrl.Should().Be("http://site.com/img.jpg"); 

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString() != null && v.ToString().Contains("Creating book") && v.ToString().Contains("Advanced C# Patterns")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
    {
        var existingBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Old Book",
            Author = "A",
            ISBN = "1234567890",
            Category = BookCategory.Fiction,
            Price = 10m,
            CreatedAt = DateTime.UtcNow
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        var request = new CreateBookProfileRequest(
            Title: "New Book",
            Author: "C",
            ISBN: "1234567890", 
            Category: BookCategory.Fiction,
            Price: 20,
            PublishedDate: DateTime.UtcNow,
            CoverImageUrl: null
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new CreateBookCommand(request), CancellationToken.None));

        ex.Message.Should().Contain("already exists");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString() != null && v.ToString().Contains("failed") && v.ToString().Contains("ISBN")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_ChildrensBookRequest_AppliesDiscountAndConditionalMapping()
    {
        decimal originalPrice = 40.00m;
        var request = new CreateBookProfileRequest(
            Title: "Happy Bunny",
            Author: "Jane Doe",
            ISBN: "0000000001",
            Category: BookCategory.Children, 
            Price: originalPrice,
            PublishedDate: DateTime.UtcNow,
            CoverImageUrl: "http://unsafe.com/img.jpg" 
        );

        var result = await _handler.Handle(new CreateBookCommand(request), CancellationToken.None);
        result.CategoryDisplayName.Should().Be("Children's Books");
        result.Price.Should().Be(originalPrice * 0.9m);
        result.CoverImageUrl.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _cache.Dispose();
    }
}