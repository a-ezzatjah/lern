namespace ServiceContract.DTO.DtoUser;

public sealed class UserProfileDto
{
    public int Id { get; init; }
    public string PhoneNumber { get; init; } = null!;
    public string? Email { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public DateOnly? BirthDate { get; init; }
    public string? NationalCode { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Role { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}
