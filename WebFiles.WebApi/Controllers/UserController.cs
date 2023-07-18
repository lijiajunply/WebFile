using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFiles.AppModel;
using WebFiles.WebApi.ModelOpera;
using WebFiles.WebApi.Data;

namespace WebFiles.WebApi.Controllers
{
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
        public async Task<ActionResult<UserAppModel>> Login(LoginModel model)
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
        public async Task<ActionResult<UserAppModel>> SignUp(LoginModel model)
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

        [TokenActionFilter]
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserDataModel userDataModel)
        {
            
            var user = _httpContextAccessor.HttpContext?.User.GetUser();
            
            if (user == null || id != user.Key || id != userDataModel.Key || user.Key != userDataModel.Key)
            {
                return BadRequest();
            }

            _context.Entry(userDataModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserModelExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [Authorize]
        [TokenActionFilter]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = _httpContextAccessor.HttpContext?.User.GetUser();

            if (user == null || id != user.Key)
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

        private bool UserModelExists(int id)
        {
            return (_context.Users?.Any(e => e.Key == id)).GetValueOrDefault();
        }
    }
}