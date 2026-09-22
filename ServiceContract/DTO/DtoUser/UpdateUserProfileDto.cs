namespace ServiceContract.DTO.DtoUser;

public sealed class UpdateUserProfileDto
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Email { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? NationalCode { get; init; }
}
