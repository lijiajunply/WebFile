using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace WebFile.BlazorServer.Data;

public class Provider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public Provider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var storageResult = await _sessionStorage.GetAsync<UserModel>("Permission");
            var model = storageResult.Success ? storageResult.Value : null;
            if (model == null)
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, model.UserName),
                new(ClaimTypes.NameIdentifier, model.Password)
            }, "Auth"));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthState(UserModel? model)
    {
        ClaimsPrincipal claimsPrincipal;
        if (model is not null)
        {
            await _sessionStorage.SetAsync("Permission", model);
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, model.UserName),
                new(ClaimTypes.NameIdentifier, model.Password)
            }));
        }
        else
        {
            await _sessionStorage.DeleteAsync("Permission");
            claimsPrincipal = _anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}