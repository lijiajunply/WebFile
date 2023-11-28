using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using WebFile.Share.Data;

namespace WebFile.BlazorServer.Providers;

public class CookiesProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private readonly HttpContext _httpContext;

    public CookiesProvider(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext.HttpContext!;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var result = await _httpContext.AuthenticateAsync();
        return result.Succeeded ? new AuthenticationState(result.Principal) : new AuthenticationState(_anonymous);
    }

    public async Task UpdateAuthState(UserModel? model)
    {
        if (model == null)
        {
            await _httpContext.SignOutAsync();
            return;
        }

        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Name, model.UserName),
            new(ClaimTypes.NameIdentifier, model.Password)
        });
        await _httpContext.SignInAsync(new ClaimsPrincipal(identity));
    }
}