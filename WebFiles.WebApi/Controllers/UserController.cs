using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFiles.AppModel;
using WebFiles.WebApi.ModelOpera;
using WebFiles.WebApi.Data;

namespace WebFiles.WebApi.Controllers;

[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]//此处为新增
[Route("[controller]/[action]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly WebFileContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserController(WebFileContext context, JwtHelper jwtHelper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Login

    [HttpPost]
    public async Task<ActionResult<UserAppModel>> Login([FromBody] LoginModel model)
    {
        if (_context.Users == null!)
        {
            return NotFound();
        }

        var result =
            await _context.Users.Include(x => x.FileModels)
                .FirstOrDefaultAsync(x => x.UserName == model.UserName && x.Password == model.Password);

        if (result == null) return NotFound();

        return ToApp.ToUser(result);
    }


    [HttpPost]
    public async Task<ActionResult<UserAppModel>> SignUp([FromBody] LoginModel model)
    {
        if (_context.Users == null!)
        {
            return NotFound();
        }

        var result =
            await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == model.UserName && x.Password == model.Password);

        if (result != null) return NotFound();

        var a = ToData.ToUser(model);

        _context.Users.Add(a);

        var path = Path.Combine(AppContext.BaseDirectory, "UserInfo", $"{a.Key}");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "Files"));
        path = Path.Combine(path, "UserImage.png");
        await System.IO.File.Create(path).DisposeAsync();

        a.UserImage = path;

        await _context.SaveChangesAsync();
        return new UserAppModel() { UserName = model.UserName };
    }

    [HttpPost]
    public async Task<ActionResult<string>> GetToken(LoginModel model)
    {
        if (_context.Users == null!)
        {
            return NotFound();
        }

        var result =
            await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == model.UserName && x.Password == model.Password);

        if (result == null) return NotFound();

        return _jwtHelper.GetMemberToken(result);
    }

    #endregion

    #region UserOpear

    [TokenActionFilter]
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ChangeModel model)
    {
        var user = _httpContextAccessor.HttpContext?.User.GetUser();

        if (user == null || id != user.Key || !IsOn(user))
        {
            return BadRequest();
        }

        var result =
            await _context.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName && x.Password == user.Password);

        if (result is null)
            return NotFound();

        if (!string.IsNullOrEmpty(model.UserName))
            result.UserName = model.UserName;
        if (!string.IsNullOrEmpty(model.Password))
            result.Password = model.Password;
        if (!string.IsNullOrEmpty(model.UserImage))
            result.UserImage = model.UserImage;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [TokenActionFilter]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = _httpContextAccessor.HttpContext?.User.GetUser();

        if (user == null || id != user.Key || !IsOn(user))
        {
            return BadRequest();
        }

        if (_context.Users == null!)
        {
            return NotFound();
        }

        var userModel = await _context.Users.FindAsync(id);
        if (userModel == null)
        {
            return NotFound();
        }

        _context.Users.Remove(userModel);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    [HttpGet("{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        var user = _httpContextAccessor.HttpContext?.User.GetUser();
        if (user == null) return NotFound();
        Stream stream = System.IO.File.OpenRead($"/UserInfo/{user.UserName}{user.Key}/{fileName}");
        return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = fileName };
    }

    [HttpPost("Upload")]
    public async Task<ActionResult> Upload(IFormFile file)
    {
        var user = _httpContextAccessor.HttpContext?.User.GetUser();
        if (user == null) return NotFound();
        var s = System.IO.File.Create($"/UserInfo/{user.UserName}{user.Key}/{file.FileName}");
        await file.CopyToAsync(s);
        return Ok();
    }

    #endregion

    private bool IsOn(UserDataModel model)
    {
        return _context.Users!.Any(x =>
            x.UserName == model.UserName || x.Password == model.Password || x.Key == model.Key);
    }
}