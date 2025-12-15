using Microsoft.AspNetCore.Mvc;
using backend.Models;
using System.Linq;
using backend.Contracts.Requests;
using backend.Services;
using Microsoft.AspNetCore.SignalR;
using backend.Hubs;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly BmsContext _context;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IDeviceDataService _deviceDataService;
        private readonly IHubContext<NotificationHub> _hubContext;
        
        public UserController(
            BmsContext context, 
            IRabbitMQService rabbitMQService,
            IDeviceDataService deviceDataService,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
            _deviceDataService = deviceDataService;
            _hubContext = hubContext;
        }

        [HttpGet("profile/{id}")]
        public IActionResult GetProfile(long id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            return Ok(new {
                id = user.Id,
                surname = user.Surname,
                name = user.Name,
                patronymic = user.Patronymic,
                phone = user.Phone,
                email = user.Email
            });
        }

        [HttpPost("profile/{id}")]
        public IActionResult UpdateProfile(long id, [FromBody] UpdateProfileRequest req)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            user.Surname = req.Surname;
            user.Name = req.Name;
            user.Patronymic = req.Patronymic;
            user.Phone = req.Phone;
            user.Email = req.Email;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("change-password/{id}")]
        public IActionResult ChangePassword(long id, [FromBody] ChangePasswordRequest req)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            if (user.Password != req.OldPassword)
                return BadRequest(new { message = "Старый пароль неверен" });
            user.Password = req.NewPassword;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("update-scan-interval")]
        public async Task<IActionResult> UpdateScanInterval([FromBody] UpdateScanIntervalRequest request)
        {
            try
            {
                // Логируем действие пользователя
                var userAction = new UserAction
                {
                    UserId = request.UserId ?? 0,
                    ActionId = 5, // ID действия "Изменение времени опроса"
                    Date = DateTime.Now,
                    Description = $"Время опроса изменено на {request.ScanIntervalMs} мс"
                };
                _context.UserActions.Add(userAction);
                _context.SaveChanges();

                // Отправляем уведомление через SignalR
                await _hubContext.Clients.Group("notifications").SendAsync("UserActionCreated", new
                {
                    id = userAction.Id,
                    userId = userAction.UserId,
                    actionId = userAction.ActionId,
                    date = userAction.Date,
                    description = userAction.Description
                });

                // Обновляем scan_interval у всех устройств
                var deviceSettings = _context.DeviceSettings.ToList();
                foreach (var setting in deviceSettings)
                {
                    setting.ScanInterval = request.ScanIntervalMs;
                }

                _context.SaveChanges();

                // Обновляем интервал опроса в DeviceDataService
                await _deviceDataService.UpdateScanInterval(request.ScanIntervalMs);

                // Отправляем сообщение в RabbitMQ
                _rabbitMQService.SendMessage("scan_interval_update", new
                {
                    scan_interval_ms = request.ScanIntervalMs,
                    timestamp = DateTime.Now,
                    user_id = request.UserId
                });

                return Ok(new { message = "Время опроса успешно обновлено" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при обновлении времени опроса: {ex.Message}" });
            }
        }
    }
} 