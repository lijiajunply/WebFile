using Microsoft.AspNetCore.Mvc;
using WebFile.Share.Data;

namespace WebFile.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly WebFileContext _context;

    public AccountController(WebFileContext context)
    {
        _context = context;
    }
}