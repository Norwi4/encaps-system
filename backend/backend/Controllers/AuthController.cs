using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Contracts.Requests;
using Microsoft.EntityFrameworkCore;

namespace backend
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BmsContext _context;
        public AuthController(BmsContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Login != null && u.Password != null &&
                u.Login == request.Login &&
                u.Password == request.Password);

            if (user == null)
                return Unauthorized(new { message = "Неверный логин или пароль" });
            
            // Получаем роль пользователя
            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .FirstOrDefaultAsync();

            string roleName = "Обычный пользователь";
            bool isAdmin = false;

            if (userRole != null)
            {
                var role = await _context.Roles
                    .Where(r => r.Id == userRole.Role)
                    .FirstOrDefaultAsync();

                if (role != null)
                {
                    roleName = role.RoleName.FirstOrDefault() ?? "Обычный пользователь";
                    isAdmin = userRole.Role == 2; // ID=2 - администратор
                }
            }

            // Создаем объект с информацией о пользователе и его роли
            var response = new
            {
                id = user.Id,
                login = user.Login,
                name = user.Name,
                surname = user.Surname,
                patronymic = user.Patronymic,
                email = user.Email,
                phone = user.Phone,
                comment = user.Comment,
                role = roleName,
                isAdmin = isAdmin
            };

            return Ok(response);
        }
    }
} 