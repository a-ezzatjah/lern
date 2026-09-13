namespace ServiceContract.DTO.DtoUser;

public sealed class RegisterUserDto
{
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
}
