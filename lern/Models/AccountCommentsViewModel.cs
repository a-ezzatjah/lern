namespace lern.Models;

public sealed record AccountCommentRow(int Id, int ProductId, string ProductName, string AuthorName,
    string? Title, string Body, bool IsApproved, bool IsBlocked, DateTime CreatedAt, int? Score);

public sealed class AccountCommentsViewModel
{
    public List<AccountCommentRow> Comments { get; init; } = new();
    public string? Status { get; init; }
    public int? Rating { get; init; }
    public string? Period { get; init; }
    public string Sort { get; init; } = "newest";
    public int Page { get; init; }
    public int TotalCount { get; init; }
    public int PageSize { get; init; } = 10;
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
