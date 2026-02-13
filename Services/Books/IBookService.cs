using WebApi.Contracts.Books;

namespace WebApi.Services.Books;

public interface IBookService
{
    Task<IReadOnlyList<BookResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<BookResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateBookResult> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken);
    Task<UpdateBookResult> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public enum CreateBookResultType
{
    Created,
    DuplicateIsbn
}

public sealed record CreateBookResult(CreateBookResultType ResultType, BookResponse? Book);

public enum UpdateBookResultType
{
    Updated,
    NotFound,
    DuplicateIsbn
}

public sealed record UpdateBookResult(UpdateBookResultType ResultType);
