namespace ServiceContract.DTO.DtoUser;

public sealed class LoginUserDto
{
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}
