using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using WebFiles.WebApi.Data;

namespace WebFiles.WebApi;

public class TokenActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var test = context.HttpContext.Request.Path;
        string? bearer = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(bearer) || !bearer.Contains("Bearer")) return;
        string[] jwt = bearer.Split(' ');
        var tokenObj = new JwtSecurityToken(jwt[1]);

        var claimsIdentity = new ClaimsIdentity(tokenObj.Claims);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        context.HttpContext.User = claimsPrincipal;
    }
}

public static class TokenHelper
{
    public static UserDataModel? GetUser(this ClaimsPrincipal claimsPrincipal)
    {
        try
        {
            var claimId = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == "Id");
            var claimName = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var claimPassword = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == "Password");
            if (claimId.IsNull() || claimName.IsNull() || claimPassword.IsNull())
            {
                return null;
            }

            return new UserDataModel()
                { Password = claimPassword.Value, UserName = claimName.Value, Key = int.Parse(claimId.Value) };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsNull(this Claim? claim) => claim == null || !string.IsNullOrEmpty(claim.Value);
}