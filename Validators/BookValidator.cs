using FluentValidation;
using WebApi.Models;

namespace WebApi.Validators;

// Retained for domain-level validation scenarios.
public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(b => b.Title).NotEmpty().MaximumLength(200);
        RuleFor(b => b.Author).NotEmpty().MaximumLength(120);
        RuleFor(b => b.Isbn).NotEmpty().MaximumLength(32);
        RuleFor(b => b.PageCount).GreaterThan(0).When(b => b.PageCount.HasValue);
    }
}
