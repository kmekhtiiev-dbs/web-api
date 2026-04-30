using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Contracts.Books;
using WebApi.Controllers;
using WebApi.Services.Books;

namespace WebApi.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookService> _bookServiceMock = new();
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        _controller = new BooksController(_bookServiceMock.Object);
    }

    private static BookResponse MakeResponse(int id = 1, string title = "Clean Code",
        string author = "Robert Martin", string isbn = "978-0132350884",
        DateOnly? publishedOn = null, int? pageCount = 431) =>
        new(id, title, author, isbn, publishedOn, pageCount);

    // ── GetAll ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns_Ok_With_Empty_List()
    {
        _bookServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<BookResponse>>()
            .Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_Returns_Ok_With_All_Books()
    {
        var books = new List<BookResponse>
        {
            MakeResponse(1, isbn: "ISBN-1"),
            MakeResponse(2, isbn: "ISBN-2")
        };

        _bookServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var result = await _controller.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<BookResponse>>()
            .Which.Should().HaveCount(2);
    }

    // ── GetById ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_Returns_Ok_With_Book_When_Found()
    {
        var book = MakeResponse();
        _bookServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var result = await _controller.GetById(1, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(book);
    }

    [Fact]
    public async Task GetById_Returns_NotFound_When_Book_Not_Found()
    {
        _bookServiceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookResponse?)null);

        var result = await _controller.GetById(999, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── Create ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Returns_CreatedAtAction_When_Successful()
    {
        var request = new CreateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, 431);
        var response = MakeResponse();

        _bookServiceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateBookResult(CreateBookResultType.Created, response));

        var result = await _controller.Create(request, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(BooksController.GetById));
        created.RouteValues!["id"].Should().Be(response.Id);
        created.Value.Should().Be(response);
    }

    [Fact]
    public async Task Create_Returns_Conflict_When_Duplicate_Isbn()
    {
        var request = new CreateBookRequest("Title", "Author", "DUPLICATE-ISBN", null, null);

        _bookServiceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateBookResult(CreateBookResultType.DuplicateIsbn, null));

        var result = await _controller.Create(request, CancellationToken.None);

        var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problem = conflict.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Title.Should().Be("Duplicate ISBN");
    }

    // ── Update ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_Returns_NoContent_When_Successful()
    {
        var request = new UpdateBookRequest("Updated Title", "Updated Author", "NEW-ISBN", null, null);

        _bookServiceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateBookResult(UpdateBookResultType.Updated));

        var result = await _controller.Update(1, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_Returns_NotFound_When_Book_Does_Not_Exist()
    {
        var request = new UpdateBookRequest("Title", "Author", "ISBN", null, null);

        _bookServiceMock
            .Setup(s => s.UpdateAsync(999, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateBookResult(UpdateBookResultType.NotFound));

        var result = await _controller.Update(999, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_Returns_Conflict_When_Duplicate_Isbn()
    {
        var request = new UpdateBookRequest("Title", "Author", "DUPLICATE-ISBN", null, null);

        _bookServiceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateBookResult(UpdateBookResultType.DuplicateIsbn));

        var result = await _controller.Update(1, request, CancellationToken.None);

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problem = conflict.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Title.Should().Be("Duplicate ISBN");
    }

    // ── Delete ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Returns_NoContent_When_Successful()
    {
        _bookServiceMock
            .Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_Returns_NotFound_When_Book_Not_Found()
    {
        _bookServiceMock
            .Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
