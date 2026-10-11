using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace lern.Infrastructure;

public sealed class StoreCookieConfiguration(SqlAuthenticationTicketStore store, TimeProvider clock) : IPostConfigureOptions<CookieAuthenticationOptions>
{
    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        if (name != CookieAuthenticationDefaults.AuthenticationScheme) return;
        options.SessionStore = store;
        options.TimeProvider = clock;
        options.ExpireTimeSpan = SqlAuthenticationTicketStore.AbsoluteTimeout;
        options.SlidingExpiration = false;
    }
}
