using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFile.Share.Data;

namespace WebFile.WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly WebFileContext _context;
    private readonly JwtHelper _jwtHelper;

    public AccountController(WebFileContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    [HttpPost]
    public async Task<ActionResult<string>> Login(UserModel model)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Equals(model));
        if (user == null) return NotFound();
        return _jwtHelper.GetMemberToken(model);
    }
    
    [HttpPost]
    public async Task<ActionResult<string>> Signup(UserModel model)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == model.UserName);
        if (user != null)
            return NotFound();
        try
        {
            _context.Users.Add(model);
            await _context.SaveChangesAsync();
        }
        catch
        {
            return NotFound();
        }
        return _jwtHelper.GetMemberToken(model);
    }
}