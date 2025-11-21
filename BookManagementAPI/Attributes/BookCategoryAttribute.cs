using BookManagementAPI.Features;

namespace BookManagementAPI.Attributes;
using System.ComponentModel.DataAnnotations;

public class BookCategoryAttribute : ValidationAttribute
{
    private readonly BookCategory[] _allowedCategories;

    public BookCategoryAttribute(params BookCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        if (value is BookCategory category)
        {
            if (_allowedCategories.Contains(category))
            {
                return ValidationResult.Success;
            }
        }
        var allowedString = string.Join(", ", _allowedCategories);
        return new ValidationResult(ErrorMessage ?? $"Category must be one of: {allowedString}");
    }
}