using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Contracts;
using System.Linq;
using backend.Helpers;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualizationController : ControllerBase
    {
        private readonly BmsContext _context;
        
        public VisualizationController(BmsContext context)
        {
            _context = context;
        }

        [HttpGet("data")]
        public ActionResult<VisualizationDataResponse> GetVisualizationData(
            [FromQuery] string period = "last2days",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] string? meterType = null,
            [FromQuery] long[]? objectIds = null,
            [FromQuery] long[]? meterIds = null,
            [FromQuery] string[]? parameters = null,
            [FromQuery] string aggregation = "hour") // minute, hour, day
        {
            try
            {
                // Определяем временной диапазон
                var (startDate, endDate) = GetDateRange(period, dateFrom, dateTo);
                
                // Если meterType не указан, определяем автоматически по выбранным устройствам
                if (string.IsNullOrEmpty(meterType) && meterIds != null && meterIds.Length > 0)
                {
                    var deviceTypes = _context.Devices
                        .Where(d => meterIds.Contains(d.Id))
                        .Select(d => d.DeviceType.Type)
                        .Distinct()
                        .ToList();
                    
                    if (deviceTypes.Count == 1)
                    {
                        meterType = deviceTypes.First();
                    }
                }
                
                // Получаем данные в зависимости от типа счетчика
                var data = meterType?.ToLower() switch
                {
                    "gas" => GetGasData(startDate, endDate, objectIds, meterIds, parameters, aggregation),
                    "electrical" => GetElectricData(startDate, endDate, objectIds, meterIds, parameters, aggregation),
                    _ => GetElectricData(startDate, endDate, objectIds, meterIds, parameters, aggregation) // по умолчанию электрические
                };

                return Ok(new VisualizationDataResponse
                {
                    Data = data,
                    Period = period,
                    DateFrom = startDate,
                    DateTo = endDate,
                    MeterType = meterType ?? "electrical",
                    ObjectIds = objectIds ?? new long[0],
                    MeterIds = meterIds ?? new long[0],
                    Parameters = parameters ?? new string[0]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("objects")]
        public ActionResult<List<VisualizationObject>> GetObjects()
        {
            var objects = _context.Objects
                .Include(o => o.Devices)
                .ThenInclude(d => d.DeviceType)
                .ToList()
                .Select(o => new VisualizationObject
                {
                    Id = o.Id,
                    Name = o.Name,
                    Devices = o.Devices
                        .OrderBy(d => d.SortId ?? d.Id) // Сортируем по SortId, если null - по Id
                        .Select(d => new VisualizationDevice
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Type = d.DeviceType?.Type ?? "Unknown",
                            SortId = d.SortId
                        }).ToList()
                }).ToList();

            return Ok(objects);
        }

        [HttpGet("parameters/{meterType}")]
        public ActionResult<List<VisualizationParameter>> GetParameters(string meterType)
        {
            var parameters = meterType.ToLower() switch
            {
                "gas" => GetGasParameters(),
                "electrical" => GetElectricParameters(),
                _ => GetElectricParameters()
            };

            return Ok(parameters);
        }

        [HttpGet("parameters-readable/{meterType}")]
        public ActionResult<List<VisualizationParameterReadable>> GetParametersReadable(string meterType)
        {
            var parameters = meterType.ToLower() switch
            {
                "gas" => GetGasParametersReadable(),
                "electrical" => GetElectricParametersReadable(),
                _ => GetElectricParametersReadable()
            };

            return Ok(parameters);
        }

        [HttpGet("device-types")]
        public ActionResult<List<DeviceType>> GetDeviceTypes()
        {
            var deviceTypes = _context.DeviceTypes.ToList();
            return Ok(deviceTypes);
        }

        private (DateTime startDate, DateTime endDate) GetDateRange(string period, DateTime? dateFrom, DateTime? dateTo)
        {
            var now = DateTime.Now;
            
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                return (dateFrom.Value, dateTo.Value);
            }

            return period switch
            {
                "today" => (now.Date, now.Date.AddDays(1).AddSeconds(-1)),
                "lastDay" => (DateTime.SpecifyKind(new DateTime(now.AddDays(-1).Year, now.AddDays(-1).Month, now.AddDays(-1).Day, 0, 0, 0), DateTimeKind.Unspecified)
                    , DateTime.SpecifyKind(new DateTime(now.AddDays(-1).Year, now.AddDays(-1).Month, now.AddDays(-1).Day, 23, 59, 59), DateTimeKind.Unspecified)),
                "last2days" => (now.AddDays(-2), now),
                "lastWeek" => (now.AddDays(-7), now),
                "last2weeks" => (now.AddDays(-14), now),
                "lastMonth" => (now.AddMonths(-1), now),
                "sinceMonthStart" => (new DateTime(now.Year, now.Month, 1), now),
                _ => (now.AddDays(-2), now) // по умолчанию последние 2 дня
            };
        }

        private List<VisualizationDataPoint> GetElectricData(DateTime startDate, DateTime endDate, long[]? objectIds, long[]? meterIds, string[]? parameters, string aggregation)
        {
            var query = _context.ElectricityDeviceData
                .Include(ed => ed.Device)
                .ThenInclude(d => d.Parent)
                .Where(ed => ed.TimeReading >= startDate && ed.TimeReading <= endDate);

            // Фильтруем по объектам
            if (objectIds != null && objectIds.Length > 0)
            {
                query = query.Where(ed => objectIds.Contains(ed.Device.Parent.Id));
            }

            // Фильтруем по счетчикам
            if (meterIds != null && meterIds.Length > 0)
            {
                query = query.Where(ed => meterIds.Contains(ed.DeviceId));
                var test = query.ToList();
            }

            var rawData = query
                .OrderBy(ed => ed.TimeReading)
                .ToList();

            if (rawData.Count == 0) return new List<VisualizationDataPoint>();

            // Создаем временные интервалы для агрегации
            var intervals = CreateTimeIntervals(startDate, endDate, aggregation);
            
            // Получаем все устройства одним запросом для оптимизации
            var deviceIds = rawData.Select(ed => ed.DeviceId).Distinct().ToArray();
            var devices = _context.Devices
                .Include(d => d.Parent)
                .Where(d => deviceIds.Contains(d.Id))
                .ToDictionary(d => d.Id, d => d);
            
            // Агрегируем данные по интервалам
            var result = new List<VisualizationDataPoint>();
            
            foreach (var interval in intervals)
            {
                foreach (var deviceId in meterIds ?? new long[0])
                {
                    var deviceData = rawData.Where(ed => ed.DeviceId == deviceId).ToList();
                    var intervalData = deviceData.Where(ed => 
                        ed.TimeReading >= interval.Start && ed.TimeReading < interval.End).ToList();
                    
                    if (intervalData.Count > 0)
                    {
                        // Берем первое значение каждого интервала
                        var firstItem = intervalData.First();
                        devices.TryGetValue(deviceId, out var device);
                        
                        result.Add(new VisualizationDataPoint
                        {
                            Timestamp = interval.Start,
                            DeviceId = deviceId,
                            DeviceName = device?.Name ?? $"Device {deviceId}",
                            ObjectName = device?.Parent?.Name ?? "Unknown",
                            Values = new Dictionary<string, decimal>
                            {
                                ["UL1N"] = firstItem.UL1N,
                                ["UL2N"] = firstItem.UL2N,
                                ["UL3N"] = firstItem.UL3N,
                                ["UL1L2"] = firstItem.UL1L2,
                                ["UL2L3"] = firstItem.UL2L3,
                                ["UL3L1"] = firstItem.UL3L1,
                                ["IL1"] = firstItem.IL1,
                                ["IL2"] = firstItem.IL2,
                                ["IL3"] = firstItem.IL3,
                                ["PL1"] = firstItem.PL1,
                                ["PL2"] = firstItem.PL2,
                                ["PL3"] = firstItem.PL3,
                                ["PSum"] = firstItem.PSum,
                                ["QL1"] = firstItem.QL1,
                                ["QL2"] = firstItem.QL2,
                                ["QL3"] = firstItem.QL3,
                                ["QSum"] = firstItem.QSum,
                                ["AllEnergy"] = firstItem.AllEnergy,
                                ["ReactiveEnergySum"] = firstItem.ReactiveEnergySum,
                                ["Freq"] = firstItem.Freq,
                                ["Aq1"] = firstItem.Aq1 ?? 0,
                                ["Aq2"] = firstItem.Aq2 ?? 0,
                                ["Aq3"] = firstItem.Aq3 ?? 0,
                                ["FundPfCf1"] = firstItem.FundPfCf1 ?? 0,
                                ["FundPfCf2"] = firstItem.FundPfCf2 ?? 0,
                                ["FundPfCf3"] = firstItem.FundPfCf3 ?? 0,
                                ["RotationField"] = firstItem.RotationField ?? 0,
                                ["RqcL1"] = firstItem.RqcL1 ?? 0,
                                ["RqcL2"] = firstItem.RqcL2 ?? 0,
                                ["RqcL3"] = firstItem.RqcL3 ?? 0,
                                ["RqdL1"] = firstItem.RqdL1 ?? 0,
                                ["RqdL2"] = firstItem.RqdL2 ?? 0,
                                ["RqdL3"] = firstItem.RqdL3 ?? 0,
                                ["ReactQIL1"] = firstItem.ReactQIL1 ?? 0,
                                ["ReactQIL2"] = firstItem.ReactQIL2 ?? 0,
                                ["ReactQIL3"] = firstItem.ReactQIL3 ?? 0,
                                ["ReactQCL1"] = firstItem.ReactQCL1 ?? 0,
                                ["ReactQCL2"] = firstItem.ReactQCL2 ?? 0,
                                ["ReactQCL3"] = firstItem.ReactQCL3 ?? 0,
                                ["HUL1"] = firstItem.HUL1 ?? 0,
                                ["HUL2"] = firstItem.HUL2 ?? 0,
                                ["HUL3"] = firstItem.HUL3 ?? 0,
                                ["HIL1"] = firstItem.HIL1 ?? 0,
                                ["HIL2"] = firstItem.HIL2 ?? 0,
                                ["HIL3"] = firstItem.HIL3 ?? 0,
                                ["Angle1"] = firstItem.Angle1 ?? 0,
                                ["Angle2"] = firstItem.Angle2 ?? 0,
                                ["Angle3"] = firstItem.Angle3 ?? 0,
                                ["AllEnergyK"] = firstItem.AllEnergyK ?? 0
                            }
                        });
                    }
                }
            }

            return result;
        }

        private List<VisualizationDataPoint> GetGasData(DateTime startDate, DateTime endDate, long[]? objectIds, long[]? meterIds, string[]? parameters, string aggregation)
        {
            var query = _context.GasDeviceData
                .Include(gd => gd.Device)
                .ThenInclude(d => d.Parent)
                .Where(gd => gd.ReadingTime >= startDate && gd.ReadingTime <= endDate);

            // Фильтруем по объектам
            if (objectIds != null && objectIds.Length > 0)
            {
                query = query.Where(gd => gd.Device.Parent != null && objectIds.Contains(gd.Device.Parent.Id));
            }

            // Фильтруем по счетчикам
            if (meterIds != null && meterIds.Length > 0)
            {
                query = query.Where(gd => meterIds.Contains(gd.DeviceId));
            }

            var rawData = query
                .OrderBy(gd => gd.ReadingTime)
                .ToList();

            var data = rawData.Select(gd => new VisualizationDataPoint
            {
                Timestamp = gd.ReadingTime,
                DeviceId = gd.DeviceId,
                DeviceName = gd.Device?.Name ?? $"Device {gd.DeviceId}",
                ObjectName = gd.Device?.Parent?.Name ?? "Unknown",
                Values = new Dictionary<string, decimal>
                {
                    ["TemperatureGas"] = gd.TemperatureGas,
                    ["WorkingVolume"] = gd.WorkingVolume,
                    ["StandardVolume"] = gd.StandardVolume,
                    ["InstantaneousFlow"] = gd.InstantaneousFlow,
                    ["BatteryLive"] = gd.BatteryLive ?? 0,
                    ["PressureGas"] = gd.PressureGas ?? 0,
                    ["Power"] = gd.Power ?? 0
                }
            }).ToList();

            return data;
        }

        private List<TimeInterval> CreateTimeIntervals(DateTime startDate, DateTime endDate, string aggregation)
        {
            var intervals = new List<TimeInterval>();
            var currentTime = startDate;
            
            // Округляем начало до начала интервала
            switch (aggregation.ToLower())
            {
                case "minute":
                    currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
                    break;
                case "hour":
                    currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0);
                    break;
                case "day":
                    currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
                    break;
            }
            
            while (currentTime <= endDate)
            {
                var endTime = currentTime;
                
                // Устанавливаем конец интервала
                switch (aggregation.ToLower())
                {
                    case "minute":
                        endTime = currentTime.AddMinutes(1);
                        break;
                    case "hour":
                        endTime = currentTime.AddHours(1);
                        break;
                    case "day":
                        endTime = currentTime.AddDays(1);
                        break;
                }
                
                intervals.Add(new TimeInterval
                {
                    Start = currentTime,
                    End = endTime
                });
                
                // Переходим к следующему интервалу
                switch (aggregation.ToLower())
                {
                    case "minute":
                        currentTime = currentTime.AddMinutes(1);
                        break;
                    case "hour":
                        currentTime = currentTime.AddHours(1);
                        break;
                    case "day":
                        currentTime = currentTime.AddDays(1);
                        break;
                }
            }
            
            return intervals;
        }

        private List<VisualizationParameter> GetElectricParameters()
        {
            return new List<VisualizationParameter>
            {
                new VisualizationParameter { 
                    Name = "Напряжение", 
                    Key = "voltage", 
                    Parameters = new[] { "UL1N", "UL2N", "UL3N", "UL1L2", "UL2L3", "UL3L1" } 
                },
                new VisualizationParameter { 
                    Name = "Ток", 
                    Key = "current", 
                    Parameters = new[] { "IL1", "IL2", "IL3" } 
                },
                new VisualizationParameter { 
                    Name = "Мощность", 
                    Key = "power", 
                    Parameters = new[] { "PL1", "PL2", "PL3", "PSum", "QL1", "QL2", "QL3", "QSum", "Aq1", "Aq2", "Aq3" } 
                },
                new VisualizationParameter { 
                    Name = "Энергия", 
                    Key = "energy", 
                    Parameters = new[] { "AllEnergy", "ReactiveEnergySum", "RqcL1", "RqcL2", "RqcL3", "RqdL1", "RqdL2", "RqdL3", "ReactQIL1", "ReactQIL2", "ReactQIL3", "ReactQCL1", "ReactQCL2", "ReactQCL3", "AllEnergyK" } 
                },
                new VisualizationParameter { 
                    Name = "Коэффициент мощности", 
                    Key = "powerFactor", 
                    Parameters = new[] { "FundPfCf1", "FundPfCf2", "FundPfCf3" } 
                },
                new VisualizationParameter { 
                    Name = "Частота", 
                    Key = "frequency", 
                    Parameters = new[] { "Freq" } 
                },
                new VisualizationParameter { 
                    Name = "Гармоники", 
                    Key = "harmonics", 
                    Parameters = new[] { "HUL1", "HUL2", "HUL3", "HIL1", "HIL2", "HIL3" } 
                },
                new VisualizationParameter { 
                    Name = "Углы", 
                    Key = "angles", 
                    Parameters = new[] { "Angle1", "Angle2", "Angle3" } 
                },
                new VisualizationParameter { 
                    Name = "Вращение", 
                    Key = "rotation", 
                    Parameters = new[] { "RotationField" } 
                }
            };
        }

        private List<VisualizationParameter> GetGasParameters()
        {
            return new List<VisualizationParameter>
            {
                new VisualizationParameter { Name = "Температура газа", Key = "temperature", Parameters = new[] { "TemperatureGas" } },
                new VisualizationParameter { Name = "Рабочий объем газа", Key = "volume", Parameters = new[] { "WorkingVolume", "StandardVolume" } },
                new VisualizationParameter { Name = "Мгновенный расход газа", Key = "flow", Parameters = new[] { "InstantaneousFlow" } },
                new VisualizationParameter { Name = "Батарея", Key = "battery", Parameters = new[] { "BatteryLive" } },
                new VisualizationParameter { Name = "Давление", Key = "pressure", Parameters = new[] { "PressureGas" } },
                new VisualizationParameter { Name = "Мощность", Key = "power", Parameters = new[] { "Power" } }
            };
        }

        private List<VisualizationParameterReadable> GetElectricParametersReadable()
        {
            return new List<VisualizationParameterReadable>
            {
                new VisualizationParameterReadable { 
                    Name = "Напряжение", 
                    Key = "voltage", 
                    Parameters = new[] { "UL1N", "UL2N", "UL3N", "UL1L2", "UL2L3", "UL3L1" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Ток", 
                    Key = "current", 
                    Parameters = new[] { "IL1", "IL2", "IL3" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Мощность", 
                    Key = "power", 
                    Parameters = new[] { "PL1", "PL2", "PL3", "PSum", "QL1", "QL2", "QL3", "QSum", "Aq1", "Aq2", "Aq3" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Энергия", 
                    Key = "energy", 
                    Parameters = new[] { "AllEnergy", "ReactiveEnergySum", "RqcL1", "RqcL2", "RqcL3", "RqdL1", "RqdL2", "RqdL3", "ReactQIL1", "ReactQIL2", "ReactQIL3", "ReactQCL1", "ReactQCL2", "ReactQCL3", "AllEnergyK" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Коэффициент мощности", 
                    Key = "powerFactor", 
                    Parameters = new[] { "FundPfCf1", "FundPfCf2", "FundPfCf3" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Частота", 
                    Key = "frequency", 
                    Parameters = new[] { "Freq" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Гармоники", 
                    Key = "harmonics", 
                    Parameters = new[] { "HUL1", "HUL2", "HUL3", "HIL1", "HIL2", "HIL3" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Углы", 
                    Key = "angles", 
                    Parameters = new[] { "Angle1", "Angle2", "Angle3" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Вращение", 
                    Key = "rotation", 
                    Parameters = new[] { "RotationField" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = NameHelper.GetParameterFullName(p), 
                            ShortName = NameHelper.GetParameterShortName(p) 
                        }).ToList()
                }
            };
        }

        private List<VisualizationParameterReadable> GetGasParametersReadable()
        {
            return new List<VisualizationParameterReadable>
            {
                new VisualizationParameterReadable { 
                    Name = "Температура газа", 
                    Key = "temperature", 
                    Parameters = new[] { "TemperatureGas" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = "Температура газа", 
                            ShortName = "Темп. газа" 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Рабочий объем газа", 
                    Key = "volume", 
                    Parameters = new[] { "WorkingVolume", "StandardVolume" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = p == "WorkingVolume" ? "Рабочий объем газа" : "Стандартный объем газа", 
                            ShortName = p == "WorkingVolume" ? "Раб. объем" : "Станд. объем" 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Мгновенный расход газа", 
                    Key = "flow", 
                    Parameters = new[] { "InstantaneousFlow" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = "Мгновенный расход газа", 
                            ShortName = "Расход газа" 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Батарея", 
                    Key = "battery", 
                    Parameters = new[] { "BatteryLive" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = "Состояние батареи", 
                            ShortName = "Батарея" 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Давление", 
                    Key = "pressure", 
                    Parameters = new[] { "PressureGas" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = "Давление газа", 
                            ShortName = "Давление" 
                        }).ToList()
                },
                new VisualizationParameterReadable { 
                    Name = "Мощность", 
                    Key = "power", 
                    Parameters = new[] { "Power" }
                        .Select(p => new VisualizationParameterItem 
                        { 
                            Code = p, 
                            FullName = "Мощность газа", 
                            ShortName = "Мощность" 
                        }).ToList()
                }
            };
        }
    }
} 