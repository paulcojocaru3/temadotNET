using BookManagementAPI.DTOs;
using BookManagementAPI.Features;
using BookManagementAPI.Middleware;
using BookManagementAPI.Validators;
using BookManagementAPI.Mappings;
using Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseInMemoryDatabase("BookStoreDb"));

builder.Services.AddAutoMapper(typeof(AdvancedBookMappingProfile).Assembly);

builder.Services.AddScoped<IValidator<CreateBookProfileRequest>, CreateBookProfileValidator>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateBookHandler>());
builder.Services.AddMemoryCache();
builder.Services.AddLogging();

var app = builder.Build();

app.UseMiddleware<CorrelationIDMiddleware>();

app.MapPost("/books", async ([FromBody] CreateBookProfileRequest request, IMediator mediator) =>
    {
        try 
        {
            var result = await mediator.Send(new CreateBookCommand(request));
            return Results.Created($"/books/{result.Id}", result);
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage }));
        }
        catch (InvalidOperationException ex) 
        {
            return Results.Conflict(ex.Message);
        }
    })
    .WithTags("Books")
    .WithName("CreateBook");

app.Run();