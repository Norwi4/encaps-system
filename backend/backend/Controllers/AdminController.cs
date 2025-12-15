using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Contracts;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly BmsContext _context;

        public AdminController(BmsContext context)
        {
            _context = context;
        }

        // GET: api/Admin/roles
        [HttpGet("roles")]
        public async Task<ActionResult<List<RoleResponse>>> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Select(r => new RoleResponse
                    {
                        Id = r.Id,
                        Name = r.RoleName.FirstOrDefault() ?? "",
                        RoleCode = r.RoleCode
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении ролей", error = ex.Message });
            }
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<ActionResult<List<AdminUserResponse>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new AdminUserResponse
                    {
                        Id = u.Id,
                        Login = u.Login,
                        Password = u.Password,
                        Email = u.Email,
                        Name = u.Name,
                        Surname = u.Surname,
                        Patronymic = u.Patronymic,
                        Phone = u.Phone,
                        Role = "", // Будет заполнено ниже
                        IsAdmin = false // Будет заполнено ниже
                    })
                    .ToListAsync();

                // Получаем роли для каждого пользователя
                foreach (var user in users)
                {
                    var userRole = await _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .FirstOrDefaultAsync();

                    if (userRole != null)
                    {
                        var role = await _context.Roles
                            .Where(r => r.Id == userRole.Role)
                            .FirstOrDefaultAsync();

                        if (role != null)
                        {
                            user.Role = role.RoleName.FirstOrDefault() ?? "";
                            user.IsAdmin = userRole.Role == 2; // ID=2 - администратор
                        }
                    }
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при получении пользователей", error = ex.Message });
            }
        }

        // POST: api/Admin/users
        [HttpPost("users")]
        public async Task<ActionResult<AdminUserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Пользователь с таким логином уже существует" });
                }
                
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                    if (existingEmail != null)
                    {
                        return BadRequest(new { message = "Пользователь с таким email уже существует" });
                    }
                }
                
                var user = new User
                {
                    Login = request.Login,
                    Password = request.Password,
                    Email = request.Email,
                    Surname = request.Surname,
                    Phone = request.Phone,
                    Name = request.Name,
                    Patronymic = request.Patronymic,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Добавляем роль пользователю
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    Role = request.RoleId,
                    AccessLevel = request.RoleId == 2 ? 'W' : 'R' // W для админа, R для обычного пользователя
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                
                var response = new AdminUserResponse
                {
                    Id = user.Id,
                    Login = user.Login,
                    Password = user.Password,
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                    Patronymic = user.Patronymic,
                    Phone = user.Phone,
                    Role = request.RoleId == 2 ? "Администратор" : "Обычный пользователь",
                    IsAdmin = request.RoleId == 2
                };

                return CreatedAtAction(nameof(GetUsers), response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при создании пользователя", error = ex.Message });
            }
        }

        // PUT: api/Admin/users/{id}
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }

                // Проверяем уникальность логина
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login && u.Id != id);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Пользователь с таким логином уже существует" });
                }

                // Проверяем уникальность email
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != id);
                    if (existingEmail != null)
                    {
                        return BadRequest(new { message = "Пользователь с таким email уже существует" });
                    }
                }

                // Обновляем данные пользователя
                user.Login = request.Login;
                user.Password = request.Password;
                user.Email = request.Email;
                user.Surname = request.Surname;
                user.Phone = request.Phone;
                user.Name = request.Name;
                user.Patronymic = request.Patronymic;

                // Обновляем роль пользователя
                var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == id);
                if (userRole != null)
                {
                    userRole.Role = request.RoleId;
                    userRole.AccessLevel = request.RoleId == 2 ? 'W' : 'R';
                }
                else
                {
                    // Если роли нет, создаем новую
                    var newUserRole = new UserRole
                    {
                        UserId = id,
                        Role = request.RoleId,
                        AccessLevel = request.RoleId == 2 ? 'W' : 'R'
                    };
                    _context.UserRoles.Add(newUserRole);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Пользователь успешно обновлен" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при обновлении пользователя", error = ex.Message });
            }
        }

        // DELETE: api/Admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }

                // Удаляем роли пользователя
                var userRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
                _context.UserRoles.RemoveRange(userRoles);

                // Удаляем пользователя
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Пользователь успешно удален" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при удалении пользователя", error = ex.Message });
            }
        }
    }
} 