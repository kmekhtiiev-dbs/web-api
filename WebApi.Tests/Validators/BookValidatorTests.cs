using FluentValidation.TestHelper;
using WebApi.Models;
using WebApi.Validators;

namespace WebApi.Tests.Validators;

public class BookValidatorTests
{
    private readonly BookValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Book_Is_Valid()
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "978-0132350884", PageCount = 431 };
        _validator.TestValidate(book).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Optional_Fields_Are_Null()
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "978-0132350884" };
        _validator.TestValidate(book).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_Title_Is_Empty(string title)
    {
        var book = new Book { Title = title, Author = "Robert Martin", Isbn = "978-0132350884" };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Title_Exceeds_MaxLength()
    {
        var book = new Book { Title = new string('A', 201), Author = "Robert Martin", Isbn = "978-0132350884" };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_Author_Is_Empty(string author)
    {
        var book = new Book { Title = "Clean Code", Author = author, Isbn = "978-0132350884" };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Should_Fail_When_Author_Exceeds_MaxLength()
    {
        var book = new Book { Title = "Clean Code", Author = new string('A', 121), Isbn = "978-0132350884" };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_Isbn_Is_Empty(string isbn)
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = isbn };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Fact]
    public void Should_Fail_When_Isbn_Exceeds_MaxLength()
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = new string('1', 33) };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_PageCount_Is_Zero_Or_Negative(int pageCount)
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "978-0132350884", PageCount = pageCount };
        _validator.TestValidate(book).ShouldHaveValidationErrorFor(x => x.PageCount);
    }

    [Fact]
    public void Should_Pass_When_PageCount_Is_Positive()
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "978-0132350884", PageCount = 1 };
        _validator.TestValidate(book).ShouldNotHaveValidationErrorFor(x => x.PageCount);
    }

    [Fact]
    public void Should_Not_Validate_PageCount_When_Null()
    {
        var book = new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "978-0132350884", PageCount = null };
        _validator.TestValidate(book).ShouldNotHaveValidationErrorFor(x => x.PageCount);
    }
}
