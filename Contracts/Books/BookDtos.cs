namespace WebApi.Contracts.Books;

public sealed record BookResponse(
    int Id,
    string Title,
    string Author,
    string Isbn,
    DateOnly? PublishedOn,
    int? PageCount);

public sealed record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    DateOnly? PublishedOn,
    int? PageCount);

public sealed record UpdateBookRequest(
    string Title,
    string Author,
    string Isbn,
    DateOnly? PublishedOn,
    int? PageCount);
