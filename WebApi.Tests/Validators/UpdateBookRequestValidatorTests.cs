using FluentValidation.TestHelper;
using WebApi.Contracts.Books;
using WebApi.Validators;

namespace WebApi.Tests.Validators;

public class UpdateBookRequestValidatorTests
{
    private readonly UpdateBookRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", "978-0132350884", new DateOnly(2008, 8, 11), 431);
        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Optional_Fields_Are_Null()
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, null);
        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Should_Fail_When_Title_Is_Empty(string? title)
    {
        var request = new UpdateBookRequest(title!, "Robert Martin", "978-0132350884", null, null);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Title_Exceeds_MaxLength()
    {
        var request = new UpdateBookRequest(new string('A', 201), "Robert Martin", "978-0132350884", null, null);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Should_Fail_When_Author_Is_Empty(string? author)
    {
        var request = new UpdateBookRequest("Clean Code", author!, "978-0132350884", null, null);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Should_Fail_When_Author_Exceeds_MaxLength()
    {
        var request = new UpdateBookRequest("Clean Code", new string('A', 121), "978-0132350884", null, null);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Should_Fail_When_Isbn_Is_Empty(string? isbn)
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", isbn!, null, null);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Fact]
    public void Should_Fail_When_Isbn_Exceeds_MaxLength()
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", new string('1', 33), null, null);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_PageCount_Is_Zero_Or_Negative(int pageCount)
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, pageCount);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.PageCount);
    }

    [Fact]
    public void Should_Pass_When_PageCount_Is_Positive()
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, 1);
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.PageCount);
    }

    [Fact]
    public void Should_Not_Validate_PageCount_When_Null()
    {
        var request = new UpdateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, null);
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.PageCount);
    }
}
