using Microsoft.EntityFrameworkCore;
using WebApi.Contracts.Books;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Services.Books;

public class BookService(ApplicationDbContext dbContext) : IBookService
{
    public async Task<IReadOnlyList<BookResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => ToResponse(b))
            .ToListAsync(cancellationToken);
    }

    public async Task<BookResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => ToResponse(b))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CreateBookResult> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var duplicateExists = await dbContext.Books.AnyAsync(b => b.Isbn == request.Isbn, cancellationToken);
        if (duplicateExists)
        {
            return new(CreateBookResultType.DuplicateIsbn, null);
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

        return new(CreateBookResultType.Created, ToResponse(book));
    }

    public async Task<UpdateBookResult> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var existingBook = await dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existingBook is null)
        {
            return new(UpdateBookResultType.NotFound);
        }

        var duplicateIsbnExists = await dbContext.Books.AnyAsync(
            b => b.Id != id && b.Isbn == request.Isbn,
            cancellationToken);

        if (duplicateIsbnExists)
        {
            return new(UpdateBookResultType.DuplicateIsbn);
        }

        existingBook.Title = request.Title;
        existingBook.Author = request.Author;
        existingBook.Isbn = request.Isbn;
        existingBook.PublishedOn = request.PublishedOn;
        existingBook.PageCount = request.PageCount;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new(UpdateBookResultType.Updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static BookResponse ToResponse(Book b) =>
        new(b.Id, b.Title, b.Author, b.Isbn, b.PublishedOn, b.PageCount);
}
