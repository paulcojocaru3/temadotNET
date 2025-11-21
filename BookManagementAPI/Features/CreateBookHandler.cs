using AutoMapper;
using BookManagementAPI.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BookManagementAPI.Features;
using Data;

namespace BookManagementAPI.Features;

public record CreateBookCommand(CreateBookProfileRequest Dto) : IRequest<BookProfileDto>;

public class CreateBookHandler : IRequestHandler<CreateBookCommand, BookProfileDto>
{
    private readonly ApplicationContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CreateBookHandler> _logger;

    public CreateBookHandler(
        ApplicationContext context,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CreateBookHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BookProfileDto> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating book: {Title} by {Author}. Category: {Category}, ISBN: {ISBN}", 
            request.Dto.Title, 
            request.Dto.Author, 
            request.Dto.Category, 
            request.Dto.ISBN);
        var isbnExists = await _context.Books
            .AnyAsync(b => b.ISBN == request.Dto.ISBN, cancellationToken);

        if (isbnExists)
        {
            _logger.LogWarning("Book creation failed. ISBN {ISBN} already exists.", request.Dto.ISBN);
            throw new InvalidOperationException($"A book with ISBN '{request.Dto.ISBN}' already exists.");
        }

        var bookEntity = _mapper.Map<Book>(request.Dto);

        _context.Books.Add(bookEntity);
        await _context.SaveChangesAsync(cancellationToken);

        _cache.Remove("all_books");
        _logger.LogInformation("Cache invalidated for key: all_books");

        var resultDto = _mapper.Map<BookProfileDto>(bookEntity);
        
        _logger.LogInformation("Book created successfully with ID: {Id}", resultDto.Id);

        return resultDto;
    }
}