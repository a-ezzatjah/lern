using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoUser;

namespace ServiceContract.Interfaces;

public interface IUserAuthService
{
    Task<ServiceResponseDto<UserProfileDto>> RegisterAsync(RegisterUserDto model);
    Task<ServiceResponseDto<UserProfileDto>> LoginAsync(LoginUserDto model);
    Task<UserProfileDto?> GetProfileAsync(int userId);
}
