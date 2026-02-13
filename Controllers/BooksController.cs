using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Contracts.Books;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await dbContext.Books
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => ToResponse(b))
            .ToListAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => ToResponse(b))
            .FirstOrDefaultAsync(cancellationToken);

        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        if (await dbContext.Books.AnyAsync(b => b.Isbn == request.Isbn, cancellationToken))
        {
            return Conflict(new ProblemDetails { Title = "Duplicate ISBN", Detail = "A book with this ISBN already exists." });
        }

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Isbn = request.Isbn,
            PublishedOn = request.PublishedOn,
            PageCount = request.PageCount
        };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, ToResponse(book));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var existingBook = await dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existingBook is null)
        {
            return NotFound();
        }

        var duplicateIsbnExists = await dbContext.Books.AnyAsync(
            b => b.Id != id && b.Isbn == request.Isbn,
            cancellationToken);

        if (duplicateIsbnExists)
        {
            return Conflict(new ProblemDetails { Title = "Duplicate ISBN", Detail = "A book with this ISBN already exists." });
        }

        existingBook.Title = request.Title;
        existingBook.Author = request.Author;
        existingBook.Isbn = request.Isbn;
        existingBook.PublishedOn = request.PublishedOn;
        existingBook.PageCount = request.PageCount;

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static BookResponse ToResponse(Book b) =>
        new(b.Id, b.Title, b.Author, b.Isbn, b.PublishedOn, b.PageCount);
}
