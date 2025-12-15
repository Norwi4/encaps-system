using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Contracts;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly BmsContext _context;

    public EventController(BmsContext context)
    {
        _context = context;
    }

    [HttpPost("log")]
    public async Task<ActionResult<EventLogListResponse>> GetEventLog([FromBody] EventLogRequest request)
    {
        try
        {
            var query = _context.Events
                .Include(e => e.EventNavigation)
                .Include(e => e.Object)
                .Include(e => e.Device)
                .AsQueryable();

            // Фильтр по группе объектов
            if (request.ObjectGroupId.HasValue)
            {
                query = query.Where(e => e.ObjectId == request.ObjectGroupId.Value);
            }

            // Фильтр по критичности (по умолчанию показываем только некритические события)
            if (request.IsCritical.HasValue)
            {
                query = query.Where(e => e.EventNavigation.IsCritical == request.IsCritical.Value);
            }
            else
            {
                // По умолчанию показываем только некритические события
                query = query.Where(e => e.EventNavigation.IsCritical == false || e.EventNavigation.IsCritical == null);
            }

            // Фильтр по периоду
            switch (request.PeriodType.ToLower())
            {
                case "new":
                    // Только новые события (за последние 24 часа)
                    var yesterday = DateTime.Now.AddDays(-1);
                    query = query.Where(e => e.Date >= yesterday);
                    break;
                case "week":
                    // За неделю
                    var weekAgo = DateTime.Now.AddDays(-7);
                    query = query.Where(e => e.Date >= weekAgo);
                    break;
                case "month":
                    // За месяц
                    var monthAgo = DateTime.Now.AddMonths(-1);
                    query = query.Where(e => e.Date >= monthAgo);
                    break;
                case "custom":
                    // За пользовательский период
                    if (request.StartDate.HasValue)
                    {
                        query = query.Where(e => e.Date >= request.StartDate.Value);
                    }
                    if (request.EndDate.HasValue)
                    {
                        var endDate = request.EndDate.Value.AddDays(1); // Включая конец дня
                        query = query.Where(e => e.Date < endDate);
                    }
                    break;
            }

            // Сортировка по дате (новые сначала)
            query = query.OrderByDescending(e => e.Date);

            var events = await query.ToListAsync();

            var response = new EventLogListResponse
            {
                Events = events.Select(e => new EventLogResponse
                {
                    Id = e.Id,
                    EventCode = e.EventNavigation?.Code ?? "N/A",
                    EventName = e.EventNavigation?.Name ?? "Неизвестное событие",
                    Date = e.Date ?? DateTime.MinValue,
                    ObjectName = e.Object?.Name ?? "Неизвестный объект",
                    ObjectAddress = e.Object?.Place ?? "Адрес не указан",
                    DeviceName = e.Device?.Name ?? "Неизвестное устройство",
                    EventDescription = e.Description ?? "Описание отсутствует"
                }).ToList(),
                TotalCount = events.Count()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении журнала событий", details = ex.Message });
        }
    }

    [HttpPost("alarms")]
    public async Task<ActionResult<EventLogListResponse>> GetAlarmLog([FromBody] EventLogRequest request)
    {
        // Для аварий всегда устанавливаем IsCritical = true
        request.IsCritical = true;
        return await GetEventLog(request);
    }

    [HttpGet("objects")]
    public async Task<ActionResult<List<object>>> GetObjects()
    {
        try
        {
            var objects = await _context.Objects
                .Select(o => new { o.Id, o.Name, o.Place })
                .ToListAsync();

            return Ok(objects);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении списка объектов", details = ex.Message });
        }
    }
} 