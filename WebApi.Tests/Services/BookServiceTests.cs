using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApi.Contracts.Books;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services.Books;

namespace WebApi.Tests.Services;

public class BookServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BookService _service;

    public BookServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _service = new BookService(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    private static Book MakeBook(string title = "Clean Code", string author = "Robert Martin",
        string isbn = "978-0132350884", DateOnly? publishedOn = null, int? pageCount = 431) =>
        new() { Title = title, Author = author, Isbn = isbn, PublishedOn = publishedOn, PageCount = pageCount };

    // ── GetAllAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_Returns_Empty_List_When_No_Books()
    {
        var result = await _service.GetAllAsync(CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_Returns_All_Books_OrderedById()
    {
        _dbContext.Books.AddRange(
            MakeBook(isbn: "ISBN-2", title: "Book B"),
            MakeBook(isbn: "ISBN-1", title: "Book A"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(b => b.Id).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllAsync_Maps_Fields_Correctly()
    {
        var publishedOn = new DateOnly(2008, 8, 11);
        _dbContext.Books.Add(MakeBook(publishedOn: publishedOn, pageCount: 431));
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(1);
        var book = result[0];
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Robert Martin");
        book.Isbn.Should().Be("978-0132350884");
        book.PublishedOn.Should().Be(publishedOn);
        book.PageCount.Should().Be(431);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Book_Not_Found()
    {
        var result = await _service.GetByIdAsync(999, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Book_When_Found()
    {
        var book = MakeBook();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetByIdAsync(book.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(book.Id);
        result.Title.Should().Be("Clean Code");
        result.Isbn.Should().Be("978-0132350884");
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_For_Deleted_Book()
    {
        var book = MakeBook();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetByIdAsync(book.Id, CancellationToken.None);
        result.Should().BeNull();
    }

    // ── CreateAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_Creates_Book_And_Returns_Created()
    {
        var request = new CreateBookRequest("Clean Code", "Robert Martin", "978-0132350884",
            new DateOnly(2008, 8, 11), 431);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.ResultType.Should().Be(CreateBookResultType.Created);
        result.Book.Should().NotBeNull();
        result.Book!.Id.Should().BeGreaterThan(0);
        result.Book.Title.Should().Be("Clean Code");
        result.Book.Author.Should().Be("Robert Martin");
        result.Book.Isbn.Should().Be("978-0132350884");
        result.Book.PublishedOn.Should().Be(new DateOnly(2008, 8, 11));
        result.Book.PageCount.Should().Be(431);
    }

    [Fact]
    public async Task CreateAsync_Persists_Book_To_Database()
    {
        var request = new CreateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, null);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        var stored = await _dbContext.Books.FindAsync(result.Book!.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task CreateAsync_Returns_DuplicateIsbn_When_Isbn_Already_Exists()
    {
        _dbContext.Books.Add(MakeBook());
        await _dbContext.SaveChangesAsync();

        var request = new CreateBookRequest("Different Title", "Different Author", "978-0132350884", null, null);
        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.ResultType.Should().Be(CreateBookResultType.DuplicateIsbn);
        result.Book.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_Does_Not_Persist_When_Duplicate_Isbn()
    {
        _dbContext.Books.Add(MakeBook());
        await _dbContext.SaveChangesAsync();
        var countBefore = await _dbContext.Books.CountAsync();

        var request = new CreateBookRequest("Other Book", "Other Author", "978-0132350884", null, null);
        await _service.CreateAsync(request, CancellationToken.None);

        var countAfter = await _dbContext.Books.CountAsync();
        countAfter.Should().Be(countBefore);
    }

    [Fact]
    public async Task CreateAsync_Creates_Book_With_Null_Optional_Fields()
    {
        var request = new CreateBookRequest("Clean Code", "Robert Martin", "978-0132350884", null, null);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.ResultType.Should().Be(CreateBookResultType.Created);
        result.Book!.PublishedOn.Should().BeNull();
        result.Book.PageCount.Should().BeNull();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_Returns_NotFound_When_Book_Does_Not_Exist()
    {
        var request = new UpdateBookRequest("Title", "Author", "ISBN", null, null);
        var result = await _service.UpdateAsync(999, request, CancellationToken.None);
        result.ResultType.Should().Be(UpdateBookResultType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_Updates_Book_And_Returns_Updated()
    {
        var book = MakeBook();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateBookRequest("New Title", "New Author", "NEW-ISBN",
            new DateOnly(2024, 1, 1), 200);

        var result = await _service.UpdateAsync(book.Id, request, CancellationToken.None);

        result.ResultType.Should().Be(UpdateBookResultType.Updated);

        var updated = await _dbContext.Books.FindAsync(book.Id);
        updated!.Title.Should().Be("New Title");
        updated.Author.Should().Be("New Author");
        updated.Isbn.Should().Be("NEW-ISBN");
        updated.PublishedOn.Should().Be(new DateOnly(2024, 1, 1));
        updated.PageCount.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAsync_Returns_DuplicateIsbn_When_Another_Book_Has_Same_Isbn()
    {
        var book1 = MakeBook(isbn: "ISBN-001");
        var book2 = MakeBook(isbn: "ISBN-002");
        _dbContext.Books.AddRange(book1, book2);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateBookRequest("Title", "Author", "ISBN-002", null, null);
        var result = await _service.UpdateAsync(book1.Id, request, CancellationToken.None);

        result.ResultType.Should().Be(UpdateBookResultType.DuplicateIsbn);
    }

    [Fact]
    public async Task UpdateAsync_Allows_Keeping_Same_Isbn()
    {
        var book = MakeBook(isbn: "SAME-ISBN");
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateBookRequest("Updated Title", "Updated Author", "SAME-ISBN", null, null);
        var result = await _service.UpdateAsync(book.Id, request, CancellationToken.None);

        result.ResultType.Should().Be(UpdateBookResultType.Updated);
    }

    [Fact]
    public async Task UpdateAsync_Does_Not_Modify_Book_When_Duplicate_Isbn()
    {
        var book1 = MakeBook(isbn: "ISBN-001", title: "Original Title");
        var book2 = MakeBook(isbn: "ISBN-002");
        _dbContext.Books.AddRange(book1, book2);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateBookRequest("Changed Title", "Author", "ISBN-002", null, null);
        await _service.UpdateAsync(book1.Id, request, CancellationToken.None);

        var unchanged = await _dbContext.Books.FindAsync(book1.Id);
        unchanged!.Title.Should().Be("Original Title");
        unchanged.Isbn.Should().Be("ISBN-001");
    }

    // ── DeleteAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_Returns_False_When_Book_Not_Found()
    {
        var result = await _service.DeleteAsync(999, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Deletes_Book_And_Returns_True()
    {
        var book = MakeBook();
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteAsync(book.Id, CancellationToken.None);

        result.Should().BeTrue();
        var deleted = await _dbContext.Books.FindAsync(book.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Does_Not_Delete_Other_Books()
    {
        var book1 = MakeBook(isbn: "ISBN-001");
        var book2 = MakeBook(isbn: "ISBN-002");
        _dbContext.Books.AddRange(book1, book2);
        await _dbContext.SaveChangesAsync();

        await _service.DeleteAsync(book1.Id, CancellationToken.None);

        var remaining = await _dbContext.Books.FindAsync(book2.Id);
        remaining.Should().NotBeNull();
    }
}
