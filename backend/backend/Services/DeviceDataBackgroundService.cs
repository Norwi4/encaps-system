using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class DeviceDataBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceDataBackgroundService> _logger;
    private DateTime _lastConsumptionUpdate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    public DeviceDataBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DeviceDataBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeviceDataBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var deviceDataService = scope.ServiceProvider.GetRequiredService<IDeviceDataService>();
                
                // Отправляем данные устройств каждые 10 секунд
                await deviceDataService.SendAverageDeviceData();
                
                // Отправляем данные расхода за сегодня каждые 10 минут (600 секунд)
                var currentTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                if ((currentTime - _lastConsumptionUpdate).TotalMinutes >= 10)
                {
                    await deviceDataService.SendConsumptionTodayData();
                    _lastConsumptionUpdate = currentTime;
                }
                
                // Ждем ровно 10 секунд до следующего цикла
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running DeviceDataBackgroundService");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _logger.LogInformation("DeviceDataBackgroundService is stopping.");
    }
} 