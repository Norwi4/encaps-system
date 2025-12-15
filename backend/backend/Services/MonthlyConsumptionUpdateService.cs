using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Services;

public class MonthlyConsumptionUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonthlyConsumptionUpdateService> _logger;
    private DateTime? _lastUpdateDate = null;

    public MonthlyConsumptionUpdateService(
        IServiceProvider serviceProvider,
        ILogger<MonthlyConsumptionUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonthlyConsumptionUpdateService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var lastDayOfMonth = DateTime.DaysInMonth(now.Year, now.Month);
                
                // Проверяем, последний ли это день месяца (в конце дня, после 23:00)
                // Или начало нового месяца (1-е число, первые минуты) - для надежности
                bool isLastDayOfMonth = now.Day == lastDayOfMonth && now.Hour >= 23;
                bool isFirstDayOfMonth = now.Day == 1 && now.Hour == 0 && now.Minute < 5;
                
                if ((isLastDayOfMonth || isFirstDayOfMonth) && 
                    (_lastUpdateDate == null || _lastUpdateDate.Value.Month != now.Month))
                {
                    _logger.LogInformation("Конец месяца. Обновление данных в consumption_by_month...");
                    
                    await UpdateMonthlyConsumption();
                    
                    _lastUpdateDate = now;
                    _logger.LogInformation("Данные в consumption_by_month успешно обновлены.");
                }
                
                // Проверяем каждую минуту
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в MonthlyConsumptionUpdateService");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("MonthlyConsumptionUpdateService is stopping.");
    }

    private async Task UpdateMonthlyConsumption()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BmsContext>();

        try
        {
            // Вычисляем предыдущий месяц
            var now = DateTime.UtcNow;
            var previousMonth = now.AddMonths(-1);
            var previousMonthStart = new DateTime(previousMonth.Year, previousMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddSeconds(-1);

            // Площадки с их device_id согласно формуле (электричество)
            var sites = new Dictionary<string, List<long>>
            {
                { "КВТ-Юг", new List<long> { 18, 25 } },
                { "ЛЦ", new List<long> { 17 } },
                { "КВТ-Восток", new List<long> { 13 } },
                { "КВТ-Север", new List<long> { 16 } },
                { "РСК", new List<long> { 35, 45 } }
            };

            // Площадки с их device_id для газа
            var gasSites = new Dictionary<string, List<long>>
            {
                { "КВТ-Юг", new List<long> { 31 } },
                { "КВТ-Восток", new List<long> { 32 } },
                { "КВТ-Север", new List<long> { 37 } }
            };

            // Для каждого устройства электричества вычисляем суммарный расход за предыдущий месяц
            foreach (var site in sites)
            {
                foreach (var deviceId in site.Value)
                {
                    // Получаем суммарный расход за предыдущий месяц из consumption_by_day
                    var monthStart = DateOnly.FromDateTime(previousMonthStart);
                    var monthEnd = DateOnly.FromDateTime(previousMonthEnd);

                    var totalConsumption = await context.ConsumptionByDay
                        .Where(c => c.DeviceId == deviceId && 
                                    c.Dt >= monthStart && 
                                    c.Dt <= monthEnd)
                        .SumAsync(c => c.Value);

                    // Проверяем, существует ли уже запись за этот месяц для этого устройства
                    var existingRecord = await context.ConsumptionByMonth
                        .FirstOrDefaultAsync(c => c.DeviceId == deviceId && 
                                                  c.Dt.Year == previousMonth.Year && 
                                                  c.Dt.Month == previousMonth.Month);

                    if (existingRecord != null)
                    {
                        // Обновляем существующую запись
                        existingRecord.Value = totalConsumption;
                        existingRecord.Dt = previousMonthStart;
                        _logger.LogInformation($"Обновлена запись для device_id={deviceId} за {previousMonth:yyyy-MM}: {totalConsumption}");
                    }
                    else
                    {
                        // Создаем новую запись
                        var newRecord = new ConsumptionByMonth
                        {
                            DeviceId = deviceId,
                            Dt = previousMonthStart,
                            Value = totalConsumption
                        };
                        context.ConsumptionByMonth.Add(newRecord);
                        _logger.LogInformation($"Создана запись для device_id={deviceId} за {previousMonth:yyyy-MM}: {totalConsumption}");
                    }
                }
            }

            // Для каждого устройства газа вычисляем суммарный расход за предыдущий месяц
            foreach (var site in gasSites)
            {
                foreach (var deviceId in site.Value)
                {
                    // Получаем суммарный расход за предыдущий месяц из consumption_by_day
                    var monthStart = DateOnly.FromDateTime(previousMonthStart);
                    var monthEnd = DateOnly.FromDateTime(previousMonthEnd);

                    var totalConsumption = await context.ConsumptionByDay
                        .Where(c => c.DeviceId == deviceId && 
                                    c.Dt >= monthStart && 
                                    c.Dt <= monthEnd)
                        .SumAsync(c => c.Value);

                    // Проверяем, существует ли уже запись за этот месяц для этого устройства
                    var existingRecord = await context.ConsumptionByMonth
                        .FirstOrDefaultAsync(c => c.DeviceId == deviceId && 
                                                  c.Dt.Year == previousMonth.Year && 
                                                  c.Dt.Month == previousMonth.Month);

                    if (existingRecord != null)
                    {
                        // Обновляем существующую запись
                        existingRecord.Value = totalConsumption;
                        existingRecord.Dt = previousMonthStart;
                        _logger.LogInformation($"Обновлена запись для газа device_id={deviceId} за {previousMonth:yyyy-MM}: {totalConsumption}");
                    }
                    else
                    {
                        // Создаем новую запись
                        var newRecord = new ConsumptionByMonth
                        {
                            DeviceId = deviceId,
                            Dt = previousMonthStart,
                            Value = totalConsumption
                        };
                        context.ConsumptionByMonth.Add(newRecord);
                        _logger.LogInformation($"Создана запись для газа device_id={deviceId} за {previousMonth:yyyy-MM}: {totalConsumption}");
                    }
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Данные в consumption_by_month успешно сохранены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных в consumption_by_month");
            throw;
        }
    }
}

