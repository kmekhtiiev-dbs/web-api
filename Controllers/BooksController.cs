using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await dbContext.Books
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create([FromBody] Book book, CancellationToken cancellationToken)
    {
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Book request, CancellationToken cancellationToken)
    {
        var existingBook = await dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existingBook is null)
        {
            return NotFound();
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
}
