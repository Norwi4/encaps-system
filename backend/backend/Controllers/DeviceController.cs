using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Contracts;
using backend.Helpers;
using backend.Services;
using backend.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly BmsContext _context;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IHubContext<NotificationHub> _hubContext;
        
        public DeviceController(BmsContext context, IRabbitMQService rabbitMQService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
            _hubContext = hubContext;
        }

        [HttpGet("dashboard")] // /api/Device/dashboard
        public ActionResult<DeviceDashboardResponse> GetDashboard()
        {
            var objects = _context.Objects
                .Select(o => new DeviceDashboardObject
                {
                    Id = o.Id,
                    Name = o.Name,
                    Devices = o.Devices
                        .OrderBy(d => d.SortId ?? d.Id) // Сортируем по SortId, если null - по Id
                        .Select(d => new DeviceDashboardDevice
                        {
                            Id = d.Id,
                            Name = d.Name,
                            StatusColor = d.Active ? "green" : "red",
                            SortId = d.SortId
                        }).ToList()
                }).ToList();

            foreach (var dev in objects.SelectMany(obj => obj.Devices))
            {
                dev.Params = GetDeviceParameters(dev.Id);
            }

            return Ok(new DeviceDashboardResponse { Objects = objects });
        }

        private List<DeviceDashboardParam> GetDeviceParameters(long deviceId)
        {
            // Получаем устройство с его типом
            var device = _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefault(d => d.Id == deviceId);

            if (device?.DeviceType == null)
                return new List<DeviceDashboardParam>();

            // Получаем VendorModel для plate_info
            Dictionary<string, PlateInfoField>? plateInfo = null;
            List<string> allowedParameters = new List<string>();
            bool isMercuryDevice = false;
            
            // Сначала пытаемся найти по Model (если Model указывает на VendorModel.Id)
            VendorModel? vendorModel = null;
            if (device.Model.HasValue)
            {
                vendorModel = _context.VendorModels
                    .FirstOrDefault(vm => vm.Id == device.Model.Value);
            }
            
            // Если не нашли по Model, ищем по VendorId
            if (vendorModel == null && device.Vendor.HasValue)
            {
                var vendor = _context.Vendors
                    .FirstOrDefault(v => v.Id == device.Vendor.Value);
                
                // Проверяем, является ли устройство счетчиком Меркурий
                if (vendor != null && vendor.Name != null && 
                    (vendor.Name.Contains("Меркурий", StringComparison.OrdinalIgnoreCase) || 
                     vendor.Name.Contains("Mercury", StringComparison.OrdinalIgnoreCase)))
                {
                    isMercuryDevice = true;
                }
                
                vendorModel = _context.VendorModels
                    .FirstOrDefault(vm => vm.VendorId == device.Vendor.Value);
            }
            
            if (vendorModel != null && !string.IsNullOrEmpty(vendorModel.PlateInfo))
            {
                plateInfo = PlateInfoHelper.ParsePlateInfo(vendorModel.PlateInfo);
                allowedParameters = PlateInfoHelper.GetFilteredParameters(plateInfo);
            }

            // Получаем параметры в зависимости от типа устройства
            var parameters = device.DeviceType.Type.ToLower() switch
            {
                "electrical" => GetElectricalDeviceParameters(deviceId, allowedParameters, plateInfo, isMercuryDevice),
                "gas" => GetGasDeviceParameters(deviceId, allowedParameters, plateInfo),
                _ => new List<DeviceDashboardParam>()
            };

            return parameters.Take(6).ToList(); // Максимум 6 параметров
        }

        private List<DeviceDashboardParam> GetElectricalDeviceParameters(long deviceId, List<string> allowedParameters, Dictionary<string, PlateInfoField>? plateInfo, bool isMercuryDevice = false)
        {
            var latestData = _context.ElectricityDeviceData
                .Where(ed => ed.DeviceId == deviceId)
                .OrderByDescending(ed => ed.TimeReading)
                .FirstOrDefault();

            if (latestData == null)
                return new List<DeviceDashboardParam>();

            var parameters = new List<DeviceDashboardParam>();
            
            // Если есть plate_info, используем только разрешенные параметры
            if (allowedParameters.Any())
            {
                foreach (var columnName in allowedParameters)
                {
                    var prop = PlateInfoHelper.GetPropertyInfo<ElectricityDeviceDatum>(columnName);
                    var plateInfoField = plateInfo?.GetValueOrDefault(columnName);
                    
                    // Если свойство найдено, получаем значение
                    decimal? decimalValue = null;
                    if (prop != null)
                    {
                        var value = prop.GetValue(latestData);
                        if (value != null)
                        {
                            if (value is decimal dec)
                                decimalValue = dec;
                            else
                                decimalValue = value as decimal?;
                        }
                    }
                    
                    // Добавляем параметр если:
                    // 1. Есть значение ИЛИ
                    // 2. Есть plateInfoField (даже если свойство не найдено или значение null)
                    if (decimalValue.HasValue || plateInfoField != null)
                    {
                        // Используем Label из plate_info для отображения, если он есть
                        var displayName = !string.IsNullOrEmpty(plateInfoField?.Label) 
                            ? plateInfoField.Label 
                            : (prop != null ? NameHelper.GetParameterShortName(prop.Name) : columnName);
                        var digits = int.TryParse(plateInfoField?.Digit, out var digitCount) 
                            ? digitCount 
                            : (prop != null ? NameHelper.GetParameterDecimalPlaces(prop.Name) : 2);
                        
                        // Конвертируем значение для отображения (делим на 1000 для мощностей и энергий)
                        var displayValue = decimalValue.HasValue 
                            ? NameHelper.ConvertToDisplayValue(decimalValue.Value, prop?.Name ?? columnName)
                            : 0;
                        
                        parameters.Add(new DeviceDashboardParam
                        {
                            Name = displayName,
                            Value = displayValue.ToString($"F{digits}")
                        });
                        Console.WriteLine($"✅ Added filtered parameter: {columnName} -> {prop?.Name ?? "NOT_FOUND"} = {decimalValue?.ToString() ?? "null"} (label: {displayName})");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Parameter {columnName} skipped: no value and no plateInfoField");
                    }
                }
            }
            
            // Для счетчиков Меркурий добавляем параметры полной мощности (если их еще нет)
            if (isMercuryDevice)
            {
                AddMercuryApparentPowerParameters(latestData, parameters, allowedParameters);
            }
            
            // Fallback на старую логику, если нет plate_info и не Меркурий
            if (!allowedParameters.Any() && !isMercuryDevice)
            {
                var priorityParams = new[] { "IL1", "IL2", "IL3", "PSum", "QSum", "AllEnergy" };
                
                foreach (var priorityParam in priorityParams)
                {
                    var prop = typeof(ElectricityDeviceDatum).GetProperty(priorityParam);
                    if (prop != null)
                    {
                        var value = prop.GetValue(latestData);
                        if (value != null && value is decimal decimalValue)
                        {
                            var displayValue = NameHelper.ConvertToDisplayValue(decimalValue, priorityParam);
                            var digits = NameHelper.GetParameterDecimalPlaces(priorityParam);
                            
                            parameters.Add(new DeviceDashboardParam
                            {
                                Name = NameHelper.GetParameterShortName(priorityParam),
                                Value = displayValue.ToString($"F{digits}")
                            });
                            Console.WriteLine($"✅ Added priority parameter: {priorityParam} = {decimalValue} -> {displayValue}");
                        }
                    }
                }
            }

            Console.WriteLine($"🎯 Device {deviceId}: Total parameters returned: {parameters.Count}");
            return parameters;
        }

        private void AddMercuryApparentPowerParameters(ElectricityDeviceDatum datum, List<DeviceDashboardParam> parameters, List<string> allowedParameters)
        {
            // Проверяем, какие параметры уже есть в списке
            var existingParameterNames = parameters
                .Select(p => p.Name)
                .ToHashSet();

            // Полная мощность по фазе L1
            if (datum.Aq1.HasValue && !existingParameterNames.Contains(NameHelper.GetParameterShortName("Aq1")) && !allowedParameters.Contains("aq1"))
            {
                var displayValue = NameHelper.ConvertToDisplayValue(datum.Aq1.Value, "Aq1");
                var digits = NameHelper.GetParameterDecimalPlaces("Aq1");
                parameters.Add(new DeviceDashboardParam
                {
                    Name = NameHelper.GetParameterShortName("Aq1"),
                    Value = displayValue.ToString($"F{digits}")
                });
            }

            // Полная мощность по фазе L2
            if (datum.Aq2.HasValue && !existingParameterNames.Contains(NameHelper.GetParameterShortName("Aq2")) && !allowedParameters.Contains("aq2"))
            {
                var displayValue = NameHelper.ConvertToDisplayValue(datum.Aq2.Value, "Aq2");
                var digits = NameHelper.GetParameterDecimalPlaces("Aq2");
                parameters.Add(new DeviceDashboardParam
                {
                    Name = NameHelper.GetParameterShortName("Aq2"),
                    Value = displayValue.ToString($"F{digits}")
                });
            }

            // Полная мощность по фазе L3
            if (datum.Aq3.HasValue && !existingParameterNames.Contains(NameHelper.GetParameterShortName("Aq3")) && !allowedParameters.Contains("aq3"))
            {
                var displayValue = NameHelper.ConvertToDisplayValue(datum.Aq3.Value, "Aq3");
                var digits = NameHelper.GetParameterDecimalPlaces("Aq3");
                parameters.Add(new DeviceDashboardParam
                {
                    Name = NameHelper.GetParameterShortName("Aq3"),
                    Value = displayValue.ToString($"F{digits}")
                });
            }

            // Полная мощность сумма (вычисляем как сумму Aq1 + Aq2 + Aq3)
            decimal aqSum = 0;
            if (datum.Aq1.HasValue) aqSum += datum.Aq1.Value;
            if (datum.Aq2.HasValue) aqSum += datum.Aq2.Value;
            if (datum.Aq3.HasValue) aqSum += datum.Aq3.Value;
            
            if (aqSum > 0 && !existingParameterNames.Contains(NameHelper.GetParameterShortName("AqSum")) && !allowedParameters.Contains("aq_sum"))
            {
                var displayValue = NameHelper.ConvertToDisplayValue(aqSum, "AqSum");
                var digits = NameHelper.GetParameterDecimalPlaces("AqSum");
                parameters.Add(new DeviceDashboardParam
                {
                    Name = NameHelper.GetParameterShortName("AqSum"),
                    Value = displayValue.ToString($"F{digits}")
                });
            }
        }

        private List<DeviceDashboardParam> GetGasDeviceParameters(long deviceId, List<string> allowedParameters, Dictionary<string, PlateInfoField>? plateInfo)
        {
            var latestData = _context.GasDeviceData
                .Where(gd => gd.DeviceId == deviceId)
                .OrderByDescending(gd => gd.ReadingTime)
                .FirstOrDefault();

            if (latestData == null)
                return new List<DeviceDashboardParam>();

            var parameters = new List<DeviceDashboardParam>();
            
            // Если есть plate_info, используем только разрешенные параметры
            if (allowedParameters.Any())
            {
                foreach (var columnName in allowedParameters)
                {
                    var prop = PlateInfoHelper.GetPropertyInfo<GasDeviceDatum>(columnName);
                    var plateInfoField = plateInfo?.GetValueOrDefault(columnName);
                    
                    // Если свойство найдено, получаем значение
                    decimal? decimalValue = null;
                    if (prop != null)
                    {
                        var value = prop.GetValue(latestData);
                        if (value != null)
                        {
                            if (value is decimal dec)
                                decimalValue = dec;
                            else
                                decimalValue = value as decimal?;
                        }
                    }
                    
                    // Добавляем параметр если:
                    // 1. Есть значение ИЛИ
                    // 2. Есть plateInfoField (даже если свойство не найдено или значение null)
                    if (decimalValue.HasValue || plateInfoField != null)
                    {
                        // Используем Label из plate_info для отображения, если он есть
                        var displayName = !string.IsNullOrEmpty(plateInfoField?.Label) 
                            ? plateInfoField.Label 
                            : (prop != null ? NameHelper.GetParameterShortName(prop.Name) : columnName);
                        var digits = int.TryParse(plateInfoField?.Digit, out var digitCount) 
                            ? digitCount 
                            : 2;
                        
                        parameters.Add(new DeviceDashboardParam
                        {
                            Name = displayName,
                            Value = decimalValue.HasValue ? decimalValue.Value.ToString($"F{digits}") : "0"
                        });
                        Console.WriteLine($"✅ Added filtered gas parameter: {columnName} -> {prop?.Name ?? "NOT_FOUND"} = {decimalValue?.ToString() ?? "null"} (label: {displayName})");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Gas parameter {columnName} skipped: no value and no plateInfoField");
                    }
                }
            }
            else
            {
                // Fallback на старую логику, если нет plate_info
                var properties = typeof(GasDeviceDatum).GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                    .Where(p => p.Name != "Id" && p.Name != "DeviceId");

                foreach (var prop in properties)
                {
                    var value = prop.GetValue(latestData);
                    if (value != null && value is decimal decimalValue)
                    {
                        parameters.Add(new DeviceDashboardParam
                        {
                            Name = NameHelper.GetParameterShortName(prop.Name),
                            Value = decimalValue.ToString("F3")
                        });
                    }
                    else if (value != null && value is DateTime dateTimeValue && prop.Name == "ReadingTime")
                    {
                        // Для времени чтения показываем только время
                        parameters.Add(new DeviceDashboardParam
                        {
                            Name = "Время",
                            Value = dateTimeValue.ToString("HH:mm:ss")
                        });
                    }
                }
            }

            return parameters;
        }

        [HttpGet("details/{id}")]
        public ActionResult<DeviceDetails> GetDeviceDetails(long id)
        {
            try
            {
                var device = _context.Devices
                    .Include(d => d.DeviceSettings)
                    .Include(d => d.Channel)
                    .FirstOrDefault(d => d.Id == id);

                if (device == null)
                    return NotFound(new { error = "Устройство не найдено" });

                var deviceSetting = device.DeviceSettings.FirstOrDefault();
                
                var response = new DeviceDetails
                {
                    Id = device.Id,
                    Name = device.Name,
                    Comment = device.Comment,
                    TrustedBefore = device.TrustedBefore,
                    IpAddress = device.Channel?.Ip,
                    NetworkPort = device.Channel?.Port,
                    KoeffTrans = deviceSetting?.KoeffTrans ?? 1.0,
                    ScanInterval = deviceSetting?.ScanInterval ?? 10000,
                    ChannelId = device.ChannelId,
                    ChannelName = device.Channel?.Name,
                    Active = device.Active,
                    SerialNo = device.SerialNo,
                    InstallationDate = device.InstallationDate?.ToDateTime(TimeOnly.MinValue),
                    LastReceive = device.LastReceive,
                    SortId = device.SortId,
                    DevAddr = deviceSetting?.DevAddr
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("device/{id}/details")]
        public ActionResult<DeviceDetailsResponse> GetDeviceDetailsWithParameters(long id)
        {
            try
            {
                var device = _context.Devices
                    .Include(d => d.DeviceType)
                    .Include(d => d.Parent)
                    .FirstOrDefault(d => d.Id == id);

                if (device == null)
                    return NotFound(new { error = "Устройство не найдено" });

                // Получаем VendorModel для plate_info
                Dictionary<string, PlateInfoField>? plateInfo = null;
                List<string> allowedParameters = new List<string>();
                bool isMercuryDevice = false;
                
                // Сначала пытаемся найти по Model (если Model указывает на VendorModel.Id)
                VendorModel? vendorModel = null;
                if (device.Model.HasValue)
                {
                    vendorModel = _context.VendorModels
                        .FirstOrDefault(vm => vm.Id == device.Model.Value);
                }
                
                // Если не нашли по Model, ищем по VendorId
                if (vendorModel == null && device.Vendor.HasValue)
                {
                    var vendor = _context.Vendors
                        .FirstOrDefault(v => v.Id == device.Vendor.Value);
                    
                    // Проверяем, является ли устройство счетчиком Меркурий
                    if (vendor != null && vendor.Name != null && 
                        (vendor.Name.Contains("Меркурий", StringComparison.OrdinalIgnoreCase) || 
                         vendor.Name.Contains("Mercury", StringComparison.OrdinalIgnoreCase)))
                    {
                        isMercuryDevice = true;
                    }
                    
                    vendorModel = _context.VendorModels
                        .FirstOrDefault(vm => vm.VendorId == device.Vendor.Value);
                }
                
                if (vendorModel != null && !string.IsNullOrEmpty(vendorModel.PlateInfo))
                {
                    Console.WriteLine($"📋 Found VendorModel for device {id}: Model={vendorModel.Id}, PlateInfo length={vendorModel.PlateInfo.Length}");
                    plateInfo = PlateInfoHelper.ParsePlateInfo(vendorModel.PlateInfo);
                    allowedParameters = PlateInfoHelper.GetFilteredParameters(plateInfo);
                    Console.WriteLine($"📋 Parsed {allowedParameters.Count} allowed parameters from plate_info");
                    if (plateInfo != null)
                    {
                        Console.WriteLine($"📋 All parameters in plate_info ({plateInfo.Count} total):");
                        foreach (var kvp in plateInfo)
                        {
                            Console.WriteLine($"   - {kvp.Key}: Label='{kvp.Value.Label}', Digit='{kvp.Value.Digit}'");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ VendorModel not found for device {id} (Vendor={device.Vendor}, Model={device.Model})");
                }

                // Получаем параметры в зависимости от типа устройства с учетом plate_info
                List<DeviceDetailParam> parameters = new List<DeviceDetailParam>();
                
                if (device.DeviceType != null)
                {
                    parameters = device.DeviceType.Type.ToLower() switch
                    {
                        "electrical" => GetElectricalDeviceDetailParametersWithPlateInfo(id, allowedParameters, plateInfo, isMercuryDevice),
                        "gas" => GetGasDeviceDetailParametersWithPlateInfo(id, allowedParameters, plateInfo),
                        _ => new List<DeviceDetailParam>()
                    };
                }

                var response = new DeviceDetailsResponse
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    ObjectName = device.Parent?.Name,
                    IsActive = device.Active,
                    LastReading = device.LastReceive,
                    Parameters = parameters
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("edit")]
        public async Task<ActionResult<DeviceEditResponse>> EditDevice([FromBody] DeviceEditRequest request)
        {
            try
            {
                var device = _context.Devices
                    .Include(d => d.DeviceSettings)
                    .Include(d => d.Channel)
                    .FirstOrDefault(d => d.Id == request.Id);

                if (device == null)
                    return NotFound(new { error = "Устройство не найдено" });

                // Обновляем основные поля устройства
                if (!string.IsNullOrEmpty(request.Name))
                    device.Name = request.Name;
                
                if (request.Comment != null)
                    device.Comment = request.Comment;
                
                if (request.TrustedBefore.HasValue)
                    device.TrustedBefore = request.TrustedBefore.Value;
                
                if (request.SortId.HasValue)
                    device.SortId = request.SortId.Value;

                // Получаем настройки устройства
                var deviceSetting = device.DeviceSettings.FirstOrDefault();

                // Обновляем DevAddr в настройках устройства
                if (request.DevAddr.HasValue && deviceSetting != null)
                    deviceSetting.DevAddr = request.DevAddr.Value;

                // Обновляем IP-адрес и порт в канале
                if (device.Channel != null)
                {
                    if (!string.IsNullOrEmpty(request.IpAddress))
                        device.Channel.Ip = request.IpAddress;
                    
                    if (request.NetworkPort.HasValue)
                        device.Channel.Port = request.NetworkPort.Value;
                }

                // Обновляем коэффициент трансформации и время опроса
                bool scanIntervalChanged = false;
                bool activeChanged = false;
                bool koeffTransChanged = false;
                long? oldScanInterval = deviceSetting?.ScanInterval;
                bool oldActive = device.Active;
                
                // Обновляем поле Active
                if (request.Active.HasValue)
                {
                    activeChanged = device.Active != request.Active.Value;
                    device.Active = request.Active.Value;
                }

                if (deviceSetting != null)
                {
                    if (request.KoeffTrans.HasValue)
                    {
                        koeffTransChanged = deviceSetting.KoeffTrans != request.KoeffTrans.Value;
                        deviceSetting.KoeffTrans = request.KoeffTrans.Value;
                    }
                    
                    if (request.ScanInterval.HasValue)
                    {
                        scanIntervalChanged = deviceSetting.ScanInterval != request.ScanInterval.Value;
                        deviceSetting.ScanInterval = request.ScanInterval.Value;
                    }
                }
                else if (request.KoeffTrans.HasValue || request.ScanInterval.HasValue)
                {
                    // Создаем новую настройку если её нет
                    scanIntervalChanged = request.ScanInterval.HasValue;
                    deviceSetting = new DeviceSetting
                    {
                        DeviceId = device.Id,
                        KoeffTrans = request.KoeffTrans ?? 1.0,
                        ScanInterval = request.ScanInterval ?? 10000, // По умолчанию 10 секунд
                        TypeLink = 2, // TCP по умолчанию
                        Parity = 'N',
                        ProtServiceCode = 0,
                        DayDataLive = 365,
                        SuccessReceive = 0,
                        BadReceive = 0
                    };
                    _context.DeviceSettings.Add(deviceSetting);
                }

                // Устанавливаем require_refresh = true при изменениях
                if (activeChanged || scanIntervalChanged || koeffTransChanged)
                {
                    device.RequireRefresh = true;
                }

                await _context.SaveChangesAsync();

                // Логируем изменения и отправляем уведомления
                if (activeChanged || scanIntervalChanged || koeffTransChanged)
                {
                    try
                    {
                        // Логируем изменения
                        if (activeChanged)
                        {
                            var userAction = new UserAction
                            {
                                UserId = request.UserId ?? 0,
                                ActionId = 7, // ID действия "Изменение статуса устройства"
                                Date = DateTime.Now,
                                Description = $"Статус устройства '{device.Name}' изменен с {(oldActive ? "включено" : "выключено")} на {(device.Active ? "включено" : "выключено")}"
                            };
                            _context.UserActions.Add(userAction);
                        }

                        if (scanIntervalChanged && request.ScanInterval.HasValue)
                        {
                            var userAction = new UserAction
                            {
                                UserId = request.UserId ?? 0,
                                ActionId = 6, // ID действия "Изменение времени опроса устройства"
                                Date = DateTime.Now,
                                Description = $"Время опроса устройства '{device.Name}' изменено с {oldScanInterval ?? 10000} мс на {request.ScanInterval.Value} мс"
                            };
                            _context.UserActions.Add(userAction);
                        }

                        if (koeffTransChanged)
                        {
                            var userAction = new UserAction
                            {
                                UserId = request.UserId ?? 0,
                                ActionId = 8, // ID действия "Изменение коэффициента трансформации"
                                Date = DateTime.Now,
                                Description = $"Коэффициент трансформации устройства '{device.Name}' изменен"
                            };
                            _context.UserActions.Add(userAction);
                        }

                        await _context.SaveChangesAsync();

                        // Отправляем уведомления через SignalR
                        var userActions = _context.UserActions
                            .Where(ua => ua.UserId == (request.UserId ?? 0) && ua.Date >= DateTime.Now.AddMinutes(-1))
                            .OrderByDescending(ua => ua.Id)
                            .Take(3)
                            .ToList();

                        foreach (var userAction in userActions)
                        {
                            await _hubContext.Clients.Group("notifications").SendAsync("UserActionCreated", new
                            {
                                id = userAction.Id,
                                userId = userAction.UserId,
                                actionId = userAction.ActionId,
                                date = userAction.Date,
                                description = userAction.Description
                            });
                        }

                        // Отправляем сообщение в RabbitMQ
                        _rabbitMQService.SendMessage("device_settings_update", new
                        {
                            device_id = device.Id,
                            device_name = device.Name,
                            active_changed = activeChanged,
                            scan_interval_changed = scanIntervalChanged,
                            koeff_trans_changed = koeffTransChanged,
                            old_scan_interval_ms = oldScanInterval,
                            new_scan_interval_ms = request.ScanInterval,
                            old_active = oldActive,
                            new_active = device.Active,
                            timestamp = DateTime.Now,
                            channel_id = device.ChannelId
                        });
                        Console.WriteLine($"RabbitMQ message sent: Device {device.Id} settings updated");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending RabbitMQ message: {ex.Message}");
                    }
                }

                var response = new DeviceEditResponse
                {
                    Success = true,
                    Message = "Устройство успешно обновлено",
                    Device = new DeviceDetails
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Comment = device.Comment,
                        TrustedBefore = device.TrustedBefore,
                        IpAddress = device.Channel?.Ip,
                        NetworkPort = device.Channel?.Port,
                        KoeffTrans = deviceSetting?.KoeffTrans ?? 1.0,
                        ScanInterval = deviceSetting?.ScanInterval ?? 10000,
                        ChannelId = device.ChannelId,
                        ChannelName = device.Channel?.Name,
                        Active = device.Active,
                        SerialNo = device.SerialNo,
                        InstallationDate = device.InstallationDate?.ToDateTime(TimeOnly.MinValue),
                        LastReceive = device.LastReceive
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private List<DeviceDetailParam> GetElectricalDeviceDetailParameters(long deviceId)
        {
            var latestData = _context.ElectricityDeviceData
                .Where(ed => ed.DeviceId == deviceId)
                .OrderByDescending(ed => ed.TimeReading)
                .FirstOrDefault();

            if (latestData == null)
                return new List<DeviceDetailParam>();

            var parameters = new List<DeviceDetailParam>();
            var properties = typeof(ElectricityDeviceDatum).GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                .Where(p => p.Name != "Id" && p.Name != "DeviceId");

            foreach (var prop in properties)
            {
                var value = prop.GetValue(latestData);
                if (value != null && value is decimal decimalValue)
                {
                    parameters.Add(new DeviceDetailParam
                    {
                        ShortName = NameHelper.GetParameterShortName(prop.Name),
                        FullName = NameHelper.GetParameterFullName(prop.Name),
                        Value = decimalValue.ToString("F3"),
                        Unit = GetParameterUnit(prop.Name)
                    });
                }
            }

            return parameters;
        }

        private List<DeviceDetailParam> GetElectricalDeviceDetailParametersWithPlateInfo(long deviceId, List<string> allowedParameters, Dictionary<string, PlateInfoField>? plateInfo, bool isMercuryDevice = false)
        {
            var latestData = _context.ElectricityDeviceData
                .Where(ed => ed.DeviceId == deviceId)
                .OrderByDescending(ed => ed.TimeReading)
                .FirstOrDefault();

            if (latestData == null)
                return new List<DeviceDetailParam>();

            var parameters = new List<DeviceDetailParam>();
            
            // Если есть plate_info, используем только разрешенные параметры
            if (allowedParameters.Any())
            {
                foreach (var columnName in allowedParameters)
                {
                    var prop = PlateInfoHelper.GetPropertyInfo<ElectricityDeviceDatum>(columnName);
                    var plateInfoField = plateInfo?.GetValueOrDefault(columnName);
                    
                    // Если свойство найдено, получаем значение
                    decimal? decimalValue = null;
                    if (prop != null)
                    {
                        var value = prop.GetValue(latestData);
                        if (value != null)
                        {
                            if (value is decimal dec)
                                decimalValue = dec;
                            else
                                decimalValue = value as decimal?;
                        }
                    }
                    
                    // Добавляем параметр если:
                    // 1. Есть значение ИЛИ
                    // 2. Есть plateInfoField (даже если свойство не найдено или значение null)
                    if (decimalValue.HasValue || plateInfoField != null)
                    {
                        // Используем Label из plate_info для FullName, если он есть
                        var displayName = !string.IsNullOrEmpty(plateInfoField?.Label) 
                            ? plateInfoField.Label 
                            : (prop != null ? NameHelper.GetParameterFullName(prop.Name) : columnName);
                        // ShortName остается стандартным
                        var shortName = prop != null 
                            ? NameHelper.GetParameterShortName(prop.Name) 
                            : columnName;
                        var digits = int.TryParse(plateInfoField?.Digit, out var digitCount) 
                            ? digitCount 
                            : (prop != null ? NameHelper.GetParameterDecimalPlaces(prop.Name) : 2);
                        
                        // Логирование для отладки
                        Console.WriteLine($"📋 Processing parameter {columnName}: prop={prop?.Name ?? "NOT_FOUND"}, value={decimalValue?.ToString() ?? "null"}, Label='{plateInfoField?.Label ?? "N/A"}'");
                        
                        // Конвертируем значение для отображения
                        var displayValue = decimalValue.HasValue 
                            ? NameHelper.ConvertToDisplayValue(decimalValue.Value, prop?.Name ?? columnName)
                            : 0;
                        
                        parameters.Add(new DeviceDetailParam
                        {
                            ShortName = shortName,
                            FullName = displayName,
                            Value = displayValue.ToString($"F{digits}"),
                            Unit = prop != null ? NameHelper.GetParameterUnit(prop.Name) : ""
                        });
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Parameter {columnName} skipped: no value and no plateInfoField");
                    }
                }
                
                Console.WriteLine($"✅ Total parameters added for device {deviceId}: {parameters.Count} (expected: {allowedParameters.Count})");
            }
            else
            {
                // Fallback на старую логику, если нет plate_info
                var properties = typeof(ElectricityDeviceDatum).GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                    .Where(p => p.Name != "Id" && p.Name != "DeviceId");

                foreach (var prop in properties)
                {
                    var value = prop.GetValue(latestData);
                    if (value != null && value is decimal decimalValue)
                    {
                        var displayValue = NameHelper.ConvertToDisplayValue(decimalValue, prop.Name);
                        var digits = NameHelper.GetParameterDecimalPlaces(prop.Name);
                        
                        parameters.Add(new DeviceDetailParam
                        {
                            ShortName = NameHelper.GetParameterShortName(prop.Name),
                            FullName = NameHelper.GetParameterFullName(prop.Name),
                            Value = displayValue.ToString($"F{digits}"),
                            Unit = NameHelper.GetParameterUnit(prop.Name)
                        });
                    }
                }
            }

            return parameters;
        }

        private List<DeviceDetailParam> GetGasDeviceDetailParameters(long deviceId)
        {
            var latestData = _context.GasDeviceData
                .Where(gd => gd.DeviceId == deviceId)
                .OrderByDescending(gd => gd.ReadingTime)
                .FirstOrDefault();

            if (latestData == null)
                return new List<DeviceDetailParam>();

            var parameters = new List<DeviceDetailParam>();
            var properties = typeof(GasDeviceDatum).GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                .Where(p => p.Name != "Id" && p.Name != "DeviceId");

            foreach (var prop in properties)
            {
                var value = prop.GetValue(latestData);
                if (value != null && value is decimal decimalValue)
                {
                    parameters.Add(new DeviceDetailParam
                    {
                        ShortName = NameHelper.GetParameterShortName(prop.Name),
                        FullName = NameHelper.GetParameterFullName(prop.Name),
                        Value = decimalValue.ToString("F3"),
                        Unit = GetGasParameterUnit(prop.Name)
                    });
                }
            }

            return parameters;
        }

        private List<DeviceDetailParam> GetGasDeviceDetailParametersWithPlateInfo(long deviceId, List<string> allowedParameters, Dictionary<string, PlateInfoField>? plateInfo)
        {
            var latestData = _context.GasDeviceData
                .Where(gd => gd.DeviceId == deviceId)
                .OrderByDescending(gd => gd.ReadingTime)
                .FirstOrDefault();

            if (latestData == null)
                return new List<DeviceDetailParam>();

            var parameters = new List<DeviceDetailParam>();
            
            // Если есть plate_info, используем только разрешенные параметры
            if (allowedParameters.Any())
            {
                foreach (var columnName in allowedParameters)
                {
                    var prop = PlateInfoHelper.GetPropertyInfo<GasDeviceDatum>(columnName);
                    var plateInfoField = plateInfo?.GetValueOrDefault(columnName);
                    
                    // Если свойство найдено, получаем значение
                    decimal? decimalValue = null;
                    if (prop != null)
                    {
                        var value = prop.GetValue(latestData);
                        if (value != null)
                        {
                            if (value is decimal dec)
                                decimalValue = dec;
                            else
                                decimalValue = value as decimal?;
                        }
                    }
                    
                    // Добавляем параметр если:
                    // 1. Есть значение ИЛИ
                    // 2. Есть plateInfoField (даже если свойство не найдено или значение null)
                    if (decimalValue.HasValue || plateInfoField != null)
                    {
                        // Используем Label из plate_info для FullName, если он есть
                        var displayName = !string.IsNullOrEmpty(plateInfoField?.Label) 
                            ? plateInfoField.Label 
                            : (prop != null ? NameHelper.GetParameterFullName(prop.Name) : columnName);
                        // ShortName остается стандартным
                        var shortName = prop != null 
                            ? NameHelper.GetParameterShortName(prop.Name) 
                            : columnName;
                        var digits = int.TryParse(plateInfoField?.Digit, out var digitCount) 
                            ? digitCount 
                            : 2;
                        
                        // Логирование для отладки
                        Console.WriteLine($"📋 Processing gas parameter {columnName}: prop={prop?.Name ?? "NOT_FOUND"}, value={decimalValue?.ToString() ?? "null"}, Label='{plateInfoField?.Label ?? "N/A"}'");
                        
                        parameters.Add(new DeviceDetailParam
                        {
                            ShortName = shortName,
                            FullName = displayName,
                            Value = decimalValue.HasValue ? decimalValue.Value.ToString($"F{digits}") : "0",
                            Unit = prop != null ? GetGasParameterUnit(prop.Name) : ""
                        });
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Gas parameter {columnName} skipped: no value and no plateInfoField");
                    }
                }
                
                Console.WriteLine($"✅ Total gas parameters added for device {deviceId}: {parameters.Count} (expected: {allowedParameters.Count})");
            }
            else
            {
                // Fallback на старую логику, если нет plate_info
                var properties = typeof(GasDeviceDatum).GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                    .Where(p => p.Name != "Id" && p.Name != "DeviceId");

                foreach (var prop in properties)
                {
                    var value = prop.GetValue(latestData);
                    if (value != null && value is decimal decimalValue)
                    {
                        parameters.Add(new DeviceDetailParam
                        {
                            ShortName = NameHelper.GetParameterShortName(prop.Name),
                            FullName = NameHelper.GetParameterFullName(prop.Name),
                            Value = decimalValue.ToString("F3"),
                            Unit = GetGasParameterUnit(prop.Name)
                        });
                    }
                }
            }

            return parameters;
        }

        private string GetParameterUnit(string parameterName)
        {
            return parameterName switch
            {
                var p when p.StartsWith("U") => "В",
                var p when p.StartsWith("I") => "А",
                var p when p.StartsWith("P") || p.StartsWith("Q") || p.StartsWith("Aq") => "Вт",
                var p when p.Contains("Energy") => "кВт⋅ч",
                "Freq" => "Гц",
                var p when p.StartsWith("FundPfCf") => "",
                var p when p.StartsWith("HU") || p.StartsWith("HI") => "%",
                var p when p.StartsWith("Angle") => "°",
                _ => ""
            };
        }

        private string GetGasParameterUnit(string parameterName)
        {
            return parameterName switch
            {
                "TemperatureGas" => "°C",
                "WorkingVolume" => "м³",
                "StandardVolume" => "м³",
                "InstantaneousFlow" => "м³/ч",
                "BatteryLive" => "%",
                "PressureGas" => "Па",
                "Power" => "Вт",
                _ => ""
            };
        }

        [HttpPut("device/{id}/update")]
        public async Task<ActionResult> UpdateDevice(long id, [FromBody] UpdateDeviceRequest request)
        {
            try
            {
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                    return NotFound(new { error = "Устройство не найдено" });

                // Обновляем комментарий если он передан
                if (request.Comment != null)
                {
                    device.Comment = request.Comment;
                }

                // Обновляем дату последней поверки если она передана
                if (request.TrustedBefore.HasValue)
                {
                    device.TrustedBefore = request.TrustedBefore.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Устройство успешно обновлено" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка при обновлении устройства", details = ex.Message });
            }
        }

        [HttpGet("scan-interval")]
        public async Task<IActionResult> GetAverageScanInterval()
        {
            try
            {
                var averageScanInterval = await _context.DeviceSettings
                    .Where(ds => ds.ScanInterval > 0)
                    .AverageAsync(ds => (double)ds.ScanInterval);

                return Ok(new { averageScanInterval = (int)Math.Round(averageScanInterval > 0 ? averageScanInterval : 5000) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Ошибка при получении времени опроса: {ex.Message}" });
            }
        }
    }
} 