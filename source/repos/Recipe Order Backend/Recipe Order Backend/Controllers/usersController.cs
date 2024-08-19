using Autorisation.Interfaces;
using Autorisation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeOrder.Data;
using RecipeOrder.Models;
using System.Security.Claims;

namespace RecipeOrder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly ApplicationUserDbContext _userDbContext;
        private readonly IUsersRepository _usersRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UsersController(UsersService usersService, ApplicationUserDbContext userDbContext, IUsersRepository usersRepository, IPasswordHasher passwordHasher)
        {
            _usersService = usersService;
            _userDbContext = userDbContext;
            _usersRepository = usersRepository;
            _passwordHasher = passwordHasher;
        }

        [Authorize]
        [HttpGet("info")]
        public async Task<ActionResult<List<User>>> GetUserInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var userInfo = await _userDbContext.Users
                                               .Where(u => u.UserID == userId)
                                               .Select(u => new
                                               {
                                                   u.UserID,
                                                   u.Username,
                                                   u.Email,
                                                   u.Sex,
                                               })
                                               .FirstOrDefaultAsync();
            return Ok(userInfo);
        }

        [Authorize]
        [HttpPost("updateInfo")]
        public async Task<ActionResult> UpdateUserInfo([FromBody] User request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var user = await _userDbContext.Users
                                 .Where(u => u.UserID == userId)
                                 .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            user.Sex = string.IsNullOrWhiteSpace(request.Sex) ? user.Sex : request.Sex;
            user.Username = string.IsNullOrWhiteSpace(request.Username) ? user.Username : request.Username;

            await _userDbContext.SaveChangesAsync();

            return Ok("Информация о пользователе обновлена");
        }

        [Authorize]
        [HttpPost("passwordChange")]
        public async Task<ActionResult> PasswordChange([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var user = await _userDbContext.Users
                                 .Where(u => u.UserID == userId)
                                 .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (!_passwordHasher.Verify(request.OldPassword, user.Password))
            {
                return BadRequest("Неверный старый пароль");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Новый пароль и подтверждение пароля не совпадают");
            }

            user.Password = _passwordHasher.Generate(request.NewPassword);
            await _userDbContext.SaveChangesAsync();

            return Ok("Пароль успешно изменен");
        }

    }

}
