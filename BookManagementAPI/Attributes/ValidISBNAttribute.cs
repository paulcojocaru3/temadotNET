using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookManagementAPI.Attributes;

public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
{
    public ValidISBNAttribute()
    {
        ErrorMessage = "Invalid ISBN format. Must be 10 or 13 digits.";
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-isbn", ErrorMessageString);
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;

        if (value is not string isbn) return false;

        var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

        if (!long.TryParse(cleanIsbn, out _)) return false;

        return cleanIsbn.Length == 10 || cleanIsbn.Length == 13;
    }
}