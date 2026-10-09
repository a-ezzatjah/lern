namespace lern.Models;

public sealed class AccountCommentEditViewModel
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = "";
    public string? Title { get; set; }
    public string Body { get; set; } = "";
    public int? Score { get; set; }
    public bool IsApproved { get; init; }
    public bool IsBlocked { get; init; }
    public string? Error { get; set; }
}
