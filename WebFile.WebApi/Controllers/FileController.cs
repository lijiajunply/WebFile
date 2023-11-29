using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFile.Share.Data;

namespace WebFile.WebApi.Controllers;

[TokenActionFilter]
[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")] //此处为新增
public class FileController : ControllerBase
{
    private readonly WebFileContext _context;
    private readonly IWebHostEnvironment _environment;

    public FileController(WebFileContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("{url}")]
    public async Task<ActionResult<List<FolderModel>>> GetData(string url)
    {
        var model = await GetUser();
        if (model == null) return NotFound();
        return model.GetFolder(url);
    }

    [HttpPost]
    public async Task<ActionResult> UploadFile([FromBody] string url, IFormFile file)
    {
        var user = await GetUser();
        if (user == null) return NotFound();
        var filePath = _environment.WebRootPath + "\\UserFiles";
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
        var fileName = $"{user.UserName}/{Convert.ToBase64String(
            System.Text.Encoding.Default.GetBytes($"{DateTime.Now:s}{Guid.NewGuid().ToString()}"))}/{file.FileName}";
        var saveFilePath = Path.Combine(filePath, fileName);

        await using var saveStream = new FileStream(saveFilePath, FileMode.OpenOrCreate);
        await file.CopyToAsync(saveStream);

        var pwd = $"{user.UserName}{DateTime.Now:s}{Guid.NewGuid().ToString()}{file.FileName}".HashEncryption()
            .Replace("/", "-");
        user.Files.Add(new FileModel() { Path = fileName, Id = pwd, Url = url });
        await _context.SaveChangesAsync();

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> Download(string id)
    {
        var user = await GetUser();
        if (user == null) return NotFound();
        var folder = user.Files.FirstOrDefault(x => x.Id == id)?.ToFolder();
        if (folder == null || folder.IsFolder) return NotFound();

        return new FileStreamResult(folder.GetStream(_environment.WebRootPath),
            new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(folder.GetMIME()))
        {
            FileDownloadName = Path.GetFileName(folder.Path)
        };
    }

    private async Task<UserModel?> GetUser()
    {
        var userModel = User.GetUser();
        if (userModel == null) return default;
        var user = await _context.Users.Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Equals(userModel));
        return user;
    }
}