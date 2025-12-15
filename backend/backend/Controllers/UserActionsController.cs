using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Contracts;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserActionsController : ControllerBase
{
    private readonly BmsContext _context;
    private readonly INotificationService _notificationService;

    public UserActionsController(BmsContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [HttpPost("log")]
    public async Task<ActionResult<UserActionsLogListResponse>> GetUserActionsLog([FromBody] UserActionsLogRequest request)
    {
        try
        {
            var query = _context.UserActions
                .Include(ua => ua.User)
                .AsQueryable();

            // Фильтр по пользователю
            if (request.UserId.HasValue)
            {
                query = query.Where(ua => ua.UserId == request.UserId.Value);
            }

            // Фильтр по типу действия
            if (request.ActionId.HasValue)
            {
                query = query.Where(ua => ua.ActionId == request.ActionId.Value);
            }

            // Фильтр по периоду
            switch (request.PeriodType.ToLower())
            {
                case "new":
                    // Только новые действия (за последние 24 часа)
                    var yesterday = DateTime.Now.AddDays(-1);
                    query = query.Where(ua => ua.Date >= yesterday);
                    break;
                case "week":
                    // За неделю
                    var weekAgo = DateTime.Now.AddDays(-7);
                    query = query.Where(ua => ua.Date >= weekAgo);
                    break;
                case "month":
                    // За месяц
                    var monthAgo = DateTime.Now.AddMonths(-1);
                    query = query.Where(ua => ua.Date >= monthAgo);
                    break;
                case "custom":
                    // За пользовательский период
                    if (request.StartDate.HasValue)
                    {
                        query = query.Where(ua => ua.Date >= request.StartDate.Value);
                    }
                    if (request.EndDate.HasValue)
                    {
                        var endDate = request.EndDate.Value.AddDays(1); // Включая конец дня
                        query = query.Where(ua => ua.Date < endDate);
                    }
                    break;
            }

            // Сортировка по дате (новые сначала)
            query = query.OrderByDescending(ua => ua.Date);

            var userActions = await query.ToListAsync();

            // Получаем информацию о действиях
            var actionIds = userActions.Select(ua => ua.ActionId).Distinct().ToList();
            var actions = await _context.Actions
                .Where(a => actionIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a);

            var response = new UserActionsLogListResponse
            {
                UserActions = userActions.Select(ua => new UserActionsLogResponse
                {
                    Id = ua.Id,
                    ActionCode = actions.ContainsKey(ua.ActionId) ? actions[ua.ActionId].Code : ua.ActionId.ToString(),
                    ActionName = actions.ContainsKey(ua.ActionId) ? actions[ua.ActionId].Name ?? "Неизвестное действие" : "Действие " + ua.ActionId,
                    Date = ua.Date,
                    UserName = ua.User?.Name ?? "Неизвестный пользователь",
                    UserSurname = ua.User?.Surname ?? "",
                    Description = ua.Description ?? "Описание отсутствует"
                }).ToList(),
                TotalCount = userActions.Count()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении журнала оповещений", details = ex.Message });
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<object>>> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Select(u => new { u.Id, Name = $"{u.Name} {u.Surname}".Trim() })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении списка пользователей", details = ex.Message });
        }
    }

    [HttpGet("actions")]
    public async Task<ActionResult<List<object>>> GetActions()
    {
        try
        {
            var actions = await _context.Actions
                .Select(a => new { a.Id, a.Code, a.Name })
                .ToListAsync();

            return Ok(actions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении списка действий", details = ex.Message });
        }
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateUserAction([FromBody] CreateUserActionRequest request)
    {
        try
        {
            var userAction = new UserAction
            {
                UserId = request.UserId,
                ActionId = request.ActionId,
                Description = request.Description,
                Date = DateTime.Now
            };

            _context.UserActions.Add(userAction);
            await _context.SaveChangesAsync();

            // Получаем информацию о действии для уведомления
            var action = await _context.Actions.FirstOrDefaultAsync(a => a.Id == request.ActionId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            
            var notification = new UserActionsLogResponse
            {
                Id = userAction.Id,
                ActionCode = action?.Code ?? request.ActionId.ToString(),
                ActionName = action?.Name ?? "Неизвестное действие",
                Date = userAction.Date,
                UserName = user?.Name ?? "Неизвестный пользователь",
                UserSurname = user?.Surname ?? "",
                Description = userAction.Description
            };

            // Отправляем уведомление через SignalR
            await _notificationService.SendUserActionNotification(notification);

            return Ok(new { message = "Действие пользователя успешно записано", id = userAction.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при создании записи действия пользователя", details = ex.Message });
        }
    }
} 