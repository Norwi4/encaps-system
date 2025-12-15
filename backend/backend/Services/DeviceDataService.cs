using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using backend.Hubs;
using backend.Models;
using backend.Helpers;
using System.Linq;

namespace backend.Services;

public interface IDeviceDataService
{
    Task SendAverageDeviceData();
    Task UpdateScanInterval(int newIntervalMs);
    Task SendConsumptionTodayData();
}

public class DeviceDataService : IDeviceDataService
{
    private readonly BmsContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<DeviceDataService> _logger;

    public DeviceDataService(
        BmsContext context, 
        IHubContext<NotificationHub> hubContext,
        ILogger<DeviceDataService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAverageDeviceData()
    {
        try
        {
            // Получаем все устройства с их типами
            var devices = await _context.Devices
                .Include(d => d.DeviceSettings)
                .Include(d => d.Parent)
                .Include(d => d.DeviceType)
                .ToListAsync();

            var latestDeviceData = new List<object>();

            foreach (var device in devices)
            {
                // Получаем plate_info для фильтрации параметров
                Dictionary<string, PlateInfoField>? plateInfo = null;
                List<string> allowedParameters = new List<string>();
                bool isMercuryDevice = false;
                
                if (device.Vendor.HasValue)
                {
                    var vendor = await _context.Vendors
                        .FirstOrDefaultAsync(v => v.Id == device.Vendor.Value);
                    
                // Проверяем, является ли устройство счетчиком Меркурий
                if (vendor != null && vendor.Name != null && 
                    (vendor.Name.Contains("Меркурий", StringComparison.OrdinalIgnoreCase) || 
                     vendor.Name.Contains("Mercury", StringComparison.OrdinalIgnoreCase)))
                {
                    isMercuryDevice = true;
                    _logger.LogInformation($"🔍 Device {device.Id} identified as Mercury device (Vendor: {vendor.Name})");
                }
                    
                    var vendorModel = await _context.VendorModels
                        .FirstOrDefaultAsync(vm => vm.VendorId == device.Vendor.Value);
                    
                    if (vendorModel != null)
                    {
                        plateInfo = PlateInfoHelper.ParsePlateInfo(vendorModel.PlateInfo);
                        allowedParameters = PlateInfoHelper.GetFilteredParameters(plateInfo);
                    }
                }

                var deviceParameters = new List<object>();

                // Получаем параметры в зависимости от типа устройства
                if (device.DeviceType?.Type.ToLower() == "electrical")
                {
                    var latestDatum = await _context.ElectricityDeviceData
                        .Where(ed => ed.DeviceId == device.Id)
                        .OrderByDescending(ed => ed.TimeReading)
                        .FirstOrDefaultAsync();

                    if (latestDatum != null)
                    {
                        // Если есть plate_info, используем только разрешенные параметры
                        if (allowedParameters.Any())
                        {
                            foreach (var columnName in allowedParameters)
                            {
                                var prop = PlateInfoHelper.GetPropertyInfo<ElectricityDeviceDatum>(columnName);
                                if (prop != null)
                                {
                                    var value = prop.GetValue(latestDatum);
                                    if (value != null && value is decimal decimalValue)
                                    {
                                        var plateInfoField = plateInfo?.GetValueOrDefault(columnName);
                                        var displayName = NameHelper.GetParameterFullName(prop.Name);
                                        var shortName = NameHelper.GetParameterShortName(prop.Name);
                                        var digits = NameHelper.GetParameterDecimalPlaces(prop.Name);
                                        
                                        // Конвертируем значение для отображения (делим на 1000 для мощностей и энергий)
                                        var displayValue = NameHelper.ConvertToDisplayValue(decimalValue, prop.Name);
                                        
                                        deviceParameters.Add(new
                                        {
                                            parameterName = displayName,
                                            parameterShortName = shortName,
                                            parameterCode = prop.Name,
                                            value = Math.Round(Convert.ToDouble(displayValue), digits),
                                            unit = NameHelper.GetParameterUnit(prop.Name),
                                            hasValue = true
                                        });
                                    }
                                }
                            }
                        }
                        
                        // Для счетчиков Меркурий добавляем параметры полной мощности (если их еще нет)
                        if (isMercuryDevice)
                        {
                            _logger.LogInformation($"🔍 Adding Mercury apparent power parameters for device {device.Id}");
                            AddMercuryApparentPowerParameters(latestDatum, deviceParameters, allowedParameters);
                            _logger.LogInformation($"✅ Device {device.Id} now has {deviceParameters.Count} parameters after adding Mercury params");
                        }
                        
                        // Fallback на старую логику, если нет plate_info и не Меркурий
                        if (!allowedParameters.Any() && !isMercuryDevice)
                        {
                            foreach (var prop in latestDatum.GetType().GetProperties())
                            {
                                if ((prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(float) ||
                                     prop.PropertyType == typeof(double?) || prop.PropertyType == typeof(decimal?) || prop.PropertyType == typeof(float?)) &&
                                    prop.Name != "Id" && prop.Name != "DeviceId" && prop.Name != "TimeReading" && prop.Name != "Device")
                                {
                                    var value = prop.GetValue(latestDatum);
                                    var numericValue = value != null ? Math.Round(Convert.ToDouble(value), 3) : 0.0;
                                    
                                    var displayValue = NameHelper.ConvertToDisplayValue(Convert.ToDecimal(numericValue), prop.Name);
                                    var digits = NameHelper.GetParameterDecimalPlaces(prop.Name);
                                    
                                    deviceParameters.Add(new
                                    {
                                        parameterName = NameHelper.GetParameterFullName(prop.Name),
                                        parameterShortName = NameHelper.GetParameterShortName(prop.Name),
                                        parameterCode = prop.Name,
                                        value = Math.Round(Convert.ToDouble(displayValue), digits),
                                        unit = NameHelper.GetParameterUnit(prop.Name),
                                        hasValue = value != null
                                    });
                                }
                            }
                        }
                    }
                }
                else if (device.DeviceType?.Type.ToLower() == "gas")
                {
                    var latestDatum = await _context.GasDeviceData
                        .Where(gd => gd.DeviceId == device.Id)
                        .OrderByDescending(gd => gd.ReadingTime)
                        .FirstOrDefaultAsync();

                    if (latestDatum != null)
                    {
                        // Если есть plate_info, используем только разрешенные параметры
                        if (allowedParameters.Any())
                        {
                            foreach (var columnName in allowedParameters)
                            {
                                var prop = PlateInfoHelper.GetPropertyInfo<GasDeviceDatum>(columnName);
                                if (prop != null)
                                {
                                    var value = prop.GetValue(latestDatum);
                                    if (value != null && value is decimal decimalValue)
                                    {
                                        var plateInfoField = plateInfo?.GetValueOrDefault(columnName);
                                        var displayName = NameHelper.GetParameterFullName(prop.Name);
                                        var shortName = NameHelper.GetParameterShortName(prop.Name);
                                        var digits = NameHelper.GetParameterDecimalPlaces(prop.Name);
                                        
                                        // Конвертируем значение для отображения (делим на 1000 для мощностей и энергий)
                                        var displayValue = NameHelper.ConvertToDisplayValue(decimalValue, prop.Name);
                                        
                                        deviceParameters.Add(new
                                        {
                                            parameterName = displayName,
                                            parameterShortName = shortName,
                                            parameterCode = prop.Name,
                                            value = Math.Round(Convert.ToDouble(displayValue), digits),
                                            unit = NameHelper.GetParameterUnit(prop.Name),
                                            hasValue = true
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Fallback на старую логику, если нет plate_info
                            foreach (var prop in latestDatum.GetType().GetProperties())
                            {
                                if ((prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(float) ||
                                     prop.PropertyType == typeof(double?) || prop.PropertyType == typeof(decimal?) || prop.PropertyType == typeof(float?)) &&
                                    prop.Name != "Id" && prop.Name != "DeviceId" && prop.Name != "ReadingTime" && prop.Name != "Device")
                                {
                                    var value = prop.GetValue(latestDatum);
                                    var numericValue = value != null ? Math.Round(Convert.ToDouble(value), 3) : 0.0;
                                    
                                    var displayValue = NameHelper.ConvertToDisplayValue(Convert.ToDecimal(numericValue), prop.Name);
                                    var digits = NameHelper.GetParameterDecimalPlaces(prop.Name);
                                    
                                    deviceParameters.Add(new
                                    {
                                        parameterName = NameHelper.GetParameterFullName(prop.Name),
                                        parameterShortName = NameHelper.GetParameterShortName(prop.Name),
                                        parameterCode = prop.Name,
                                        value = Math.Round(Convert.ToDouble(displayValue), digits),
                                        unit = NameHelper.GetParameterUnit(prop.Name),
                                        hasValue = value != null
                                    });
                                }
                            }
                        }
                    }
                }

                if (deviceParameters.Any())
                {
                    latestDeviceData.Add(new
                    {
                        deviceId = device.Id,
                        deviceName = device.Name,
                        objectName = device.Parent?.Name,
                        statusColor = device.Active ? "green" : "red",
                        sortId = device.SortId, // Добавляем SortId для сортировки на фронтенде
                        averageValues = deviceParameters.ToDictionary(p => ((dynamic)p).parameterCode, p => ((dynamic)p).value), // Для совместимости с фронтендом
                        parameters = deviceParameters, // Полная информация о параметрах
                        lastUpdate = DateTime.Now
                    });
                }
            }

            // Отправляем данные через SignalR
            await _hubContext.Clients.Group("notifications").SendAsync("DeviceDataUpdate", latestDeviceData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке данных устройств");
        }
    }

    private void AddMercuryApparentPowerParameters(ElectricityDeviceDatum datum, List<object> deviceParameters, List<string> allowedParameters)
    {
        // Проверяем, какие параметры уже есть в списке
        var existingParameterCodes = deviceParameters
            .Select(p => ((dynamic)p).parameterCode?.ToString())
            .Where(code => !string.IsNullOrEmpty(code))
            .ToHashSet();

        _logger.LogInformation($"🔍 Adding Mercury parameters. Existing codes: {string.Join(", ", existingParameterCodes)}");
        _logger.LogInformation($"🔍 Allowed parameters: {string.Join(", ", allowedParameters)}");
        _logger.LogInformation($"🔍 Aq1: {datum.Aq1}, Aq2: {datum.Aq2}, Aq3: {datum.Aq3}");

        // Полная мощность по фазе L1
        if (datum.Aq1.HasValue && !existingParameterCodes.Contains("Aq1") && !allowedParameters.Contains("aq1"))
        {
            var displayValue = NameHelper.ConvertToDisplayValue(datum.Aq1.Value, "Aq1");
            var digits = NameHelper.GetParameterDecimalPlaces("Aq1");
            deviceParameters.Add(new
            {
                parameterName = NameHelper.GetParameterFullName("Aq1"),
                parameterShortName = NameHelper.GetParameterShortName("Aq1"),
                parameterCode = "Aq1",
                value = Math.Round(Convert.ToDouble(displayValue), digits),
                unit = NameHelper.GetParameterUnit("Aq1"),
                hasValue = true
            });
        }

        // Полная мощность по фазе L2
        if (datum.Aq2.HasValue && !existingParameterCodes.Contains("Aq2") && !allowedParameters.Contains("aq2"))
        {
            var displayValue = NameHelper.ConvertToDisplayValue(datum.Aq2.Value, "Aq2");
            var digits = NameHelper.GetParameterDecimalPlaces("Aq2");
            deviceParameters.Add(new
            {
                parameterName = NameHelper.GetParameterFullName("Aq2"),
                parameterShortName = NameHelper.GetParameterShortName("Aq2"),
                parameterCode = "Aq2",
                value = Math.Round(Convert.ToDouble(displayValue), digits),
                unit = NameHelper.GetParameterUnit("Aq2"),
                hasValue = true
            });
        }

        // Полная мощность по фазе L3
        if (datum.Aq3.HasValue && !existingParameterCodes.Contains("Aq3") && !allowedParameters.Contains("aq3"))
        {
            var displayValue = NameHelper.ConvertToDisplayValue(datum.Aq3.Value, "Aq3");
            var digits = NameHelper.GetParameterDecimalPlaces("Aq3");
            deviceParameters.Add(new
            {
                parameterName = NameHelper.GetParameterFullName("Aq3"),
                parameterShortName = NameHelper.GetParameterShortName("Aq3"),
                parameterCode = "Aq3",
                value = Math.Round(Convert.ToDouble(displayValue), digits),
                unit = NameHelper.GetParameterUnit("Aq3"),
                hasValue = true
            });
        }

        // Полная мощность сумма (вычисляем как сумму Aq1 + Aq2 + Aq3)
        decimal aqSum = 0;
        if (datum.Aq1.HasValue) aqSum += datum.Aq1.Value;
        if (datum.Aq2.HasValue) aqSum += datum.Aq2.Value;
        if (datum.Aq3.HasValue) aqSum += datum.Aq3.Value;
        
        if (aqSum > 0 && !existingParameterCodes.Contains("AqSum") && !allowedParameters.Contains("aq_sum"))
        {
            var displayValue = NameHelper.ConvertToDisplayValue(aqSum, "AqSum");
            var digits = NameHelper.GetParameterDecimalPlaces("AqSum");
            deviceParameters.Add(new
            {
                parameterName = NameHelper.GetParameterFullName("AqSum"),
                parameterShortName = NameHelper.GetParameterShortName("AqSum"),
                parameterCode = "AqSum",
                value = Math.Round(Convert.ToDouble(displayValue), digits),
                unit = NameHelper.GetParameterUnit("AqSum"),
                hasValue = true
            });
        }
    }

    public async Task UpdateScanInterval(int newIntervalMs)
    {
        try
        {
            // Обновляем scan_interval у всех устройств в базе данных
            var deviceSettings = await _context.DeviceSettings.ToListAsync();
            
            foreach (var setting in deviceSettings)
            {
                setting.ScanInterval = newIntervalMs;
            }
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Scan interval updated to: {newIntervalMs}ms in database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scan interval in database");
        }
    }

    public async Task SendConsumptionTodayData()
    {
        try
        {
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            var todayStart = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0), DateTimeKind.Utc);
            
            // Площадки с их device_id согласно формуле
            var sites = new Dictionary<string, List<long>>
            {
                { "КВТ-Юг", new List<long> { 18, 25 } },
                { "ЛЦ", new List<long> { 17 } },
                { "КВТ-Восток", new List<long> { 13 } },
                { "КВТ-Север", new List<long> { 16 } },
                { "РСК", new List<long> { 35, 45 } }
            };

            var consumptionData = new List<object>();

            foreach (var site in sites)
            {
                decimal totalValue = 0;

                foreach (var deviceId in site.Value)
                {
                    var value = await _context.ConsumptionByToday
                        .Where(c => c.DeviceId == deviceId && 
                                    c.Dt >= todayStart && 
                                    c.Dt <= now)
                        .OrderByDescending(c => c.Dt)
                        .Select(c => c.Value)
                        .FirstOrDefaultAsync();

                    totalValue += value;
                }

                consumptionData.Add(new
                {
                    Site = site.Key,
                    Value = totalValue
                });
            }

            // Отправляем данные через SignalR
            await _hubContext.Clients.Group("notifications").SendAsync("UpdateConsumptionToday", consumptionData);
            _logger.LogInformation("Consumption today data sent via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке данных расхода за сегодня");
        }
    }
} 