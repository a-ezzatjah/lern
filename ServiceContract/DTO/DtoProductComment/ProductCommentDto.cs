namespace ServiceContract.DTO.DtoProductComment;

public sealed class ProductCommentDto
{
    public int Id { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
