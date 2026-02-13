using FluentValidation;
using WebApi.Contracts.Books;

namespace WebApi.Validators;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(b => b.Title).NotEmpty().MaximumLength(200);
        RuleFor(b => b.Author).NotEmpty().MaximumLength(120);
        RuleFor(b => b.Isbn).NotEmpty().MaximumLength(32);
        RuleFor(b => b.PageCount).GreaterThan(0).When(b => b.PageCount.HasValue);
    }
}

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(b => b.Title).NotEmpty().MaximumLength(200);
        RuleFor(b => b.Author).NotEmpty().MaximumLength(120);
        RuleFor(b => b.Isbn).NotEmpty().MaximumLength(32);
        RuleFor(b => b.PageCount).GreaterThan(0).When(b => b.PageCount.HasValue);
    }
}
