using Microsoft.AspNetCore.Mvc;
using WebApi.Contracts.Books;
using WebApi.Services.Books;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await bookService.GetAllAsync(cancellationToken);
        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await bookService.CreateAsync(request, cancellationToken);

        if (result.ResultType == CreateBookResultType.DuplicateIsbn)
        {
            return Conflict(new ProblemDetails { Title = "Duplicate ISBN", Detail = "A book with this ISBN already exists." });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Book!.Id }, result.Book);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await bookService.UpdateAsync(id, request, cancellationToken);

        return result.ResultType switch
        {
            UpdateBookResultType.NotFound => NotFound(),
            UpdateBookResultType.DuplicateIsbn => Conflict(new ProblemDetails
            {
                Title = "Duplicate ISBN",
                Detail = "A book with this ISBN already exists."
            }),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await bookService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
