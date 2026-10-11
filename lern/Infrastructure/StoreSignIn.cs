using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using ServiceContract.DTO.DtoUser;

namespace lern.Infrastructure;

public static class StoreSignIn
{
    public static ClaimsPrincipal Principal(UserProfileDto user) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber), new Claim(ClaimTypes.Role, user.Role),
        new Claim(SqlAuthenticationTicketStore.VersionClaim, user.AuthenticationVersion.ToString())
    }, CookieAuthenticationDefaults.AuthenticationScheme));
}
