using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Contracts;
using backend.Models;
using OfficeOpenXml;
using System.IO;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly BmsContext _context;
    private readonly IWebHostEnvironment _environment;

    public ReportController(BmsContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    private string FormatDecimal(decimal value, int decimals = 3)
    {
        // Используем инвариантную культуру и заменяем точку на запятую
        var formatted = value.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
        return formatted.Replace(".", ",");
    }

    [HttpGet("list")]
    public async Task<ActionResult<ReportListResponse>> GetReports(
        [FromQuery] string? sortBy = "date", 
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] string? periodType = "new",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            Console.WriteLine($"Получены параметры: sortBy='{sortBy}', sortOrder='{sortOrder}', periodType='{periodType}', startDate={startDate}, endDate={endDate}");
            
            var baseQuery = _context.Reports
                .Include(r => r.CreatedByUser);

            // Применяем фильтрацию по дате
            IQueryable<Report> reportsQuery = baseQuery;
            
            // Фильтрация по пользовательскому периоду
            if (startDate.HasValue && endDate.HasValue)
            {
                reportsQuery = baseQuery.Where(r => r.CreatedAt >= startDate.Value && r.CreatedAt <= endDate.Value);
                Console.WriteLine($"Фильтр: пользовательский период с {startDate} по {endDate}");
            }
            else
            {
                Console.WriteLine("Фильтр: без ограничений по дате");
            }

            // Применяем сортировку
            IQueryable<Report> sortedQuery;
            switch (sortBy?.ToLower())
            {
                case "date":
                    Console.WriteLine("Применяем сортировку по дате");
                    sortedQuery = sortOrder?.ToLower() == "asc" 
                        ? reportsQuery.OrderBy(r => r.CreatedAt)
                        : reportsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
                case "name":
                    Console.WriteLine("Применяем сортировку по названию");
                    sortedQuery = sortOrder?.ToLower() == "asc"
                        ? reportsQuery.OrderBy(r => r.Name)
                        : reportsQuery.OrderByDescending(r => r.Name);
                    break;
                case "size":
                    Console.WriteLine("Применяем сортировку по размеру");
                    sortedQuery = sortOrder?.ToLower() == "asc"
                        ? reportsQuery.OrderBy(r => r.Size)
                        : reportsQuery.OrderByDescending(r => r.Size);
                    break;
                default:
                    Console.WriteLine("Применяем сортировку по умолчанию (дата desc)");
                    sortedQuery = reportsQuery.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            var reportsData = await sortedQuery.ToListAsync(); // Materialize data here

            var reports = reportsData.Select(r => new ReportResponse
            {
                Id = r.Id,
                Type = r.Type,
                Name = r.Name,
                Size = r.Size,
                Path = r.Path,
                CreatedAt = r.CreatedAt ?? DateTime.Now,
                CreatedByUserName = r.CreatedByUser?.Name ?? "Неизвестный пользователь",
                CreatedByUserSurname = r.CreatedByUser?.Surname ?? ""
            }).ToList();

            return Ok(new ReportListResponse
            {
                Reports = reports,
                TotalCount = reports.Count()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при получении списка отчетов", details = ex.Message });
        }
    }

    [HttpPost("create")]
    public async Task<ActionResult<ReportResponse>> CreateReport([FromBody] CreateReportRequest request)
    {
        try
        {
            // Создаем Excel файл
            var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var reportsPath = Path.Combine(_environment.ContentRootPath, "Reports");
            
            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }

            var filePath = Path.Combine(reportsPath, fileName);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Данные");
                
                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Дата и время";
                worksheet.Cells[1, 2].Value = "U_L1-N";
                worksheet.Cells[1, 3].Value = "U_L2-N";
                worksheet.Cells[1, 4].Value = "U_L3-N";
                worksheet.Cells[1, 5].Value = "U_L1-L2";
                worksheet.Cells[1, 6].Value = "U_L2-L3";
                worksheet.Cells[1, 7].Value = "U_L3-L1";
                worksheet.Cells[1, 8].Value = "I_L1";
                worksheet.Cells[1, 9].Value = "I_L2";
                worksheet.Cells[1, 10].Value = "I_L3";
                worksheet.Cells[1, 11].Value = "P_L1";
                worksheet.Cells[1, 12].Value = "P_L2";
                worksheet.Cells[1, 13].Value = "P_L3";
                worksheet.Cells[1, 14].Value = "P_Sum";
                worksheet.Cells[1, 15].Value = "Q_L1";
                worksheet.Cells[1, 16].Value = "Q_L2";
                worksheet.Cells[1, 17].Value = "Q_L3";
                worksheet.Cells[1, 18].Value = "Q_Sum";
                worksheet.Cells[1, 19].Value = "Energy";
                worksheet.Cells[1, 20].Value = "Reactive Energy";
                worksheet.Cells[1, 21].Value = "Frequency";

                // Стилизуем заголовки
                using (var range = worksheet.Cells[1, 1, 1, 21])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Получаем реальные данные из базы (пример с ElectricityDeviceData)
                var data = await _context.ElectricityDeviceData
                    .OrderByDescending(d => d.TimeReading)
                    .Take(100)
                    .ToListAsync();

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.TimeReading.ToString("dd.MM.yyyy HH:mm:ss");
                    worksheet.Cells[row, 2].Value = FormatDecimal(item.UL1N);
                    worksheet.Cells[row, 3].Value = FormatDecimal(item.UL2N);
                    worksheet.Cells[row, 4].Value = FormatDecimal(item.UL3N);
                    worksheet.Cells[row, 5].Value = FormatDecimal(item.UL1L2);
                    worksheet.Cells[row, 6].Value = FormatDecimal(item.UL2L3);
                    worksheet.Cells[row, 7].Value = FormatDecimal(item.UL3L1);
                    worksheet.Cells[row, 8].Value = FormatDecimal(item.IL1);
                    worksheet.Cells[row, 9].Value = FormatDecimal(item.IL2);
                    worksheet.Cells[row, 10].Value = FormatDecimal(item.IL3);
                    worksheet.Cells[row, 11].Value = FormatDecimal(item.PL1);
                    worksheet.Cells[row, 12].Value = FormatDecimal(item.PL2);
                    worksheet.Cells[row, 13].Value = FormatDecimal(item.PL3);
                    worksheet.Cells[row, 14].Value = FormatDecimal(item.PSum);
                    worksheet.Cells[row, 15].Value = FormatDecimal(item.QL1);
                    worksheet.Cells[row, 16].Value = FormatDecimal(item.QL2);
                    worksheet.Cells[row, 17].Value = FormatDecimal(item.QL3);
                    worksheet.Cells[row, 18].Value = FormatDecimal(item.QSum);
                    worksheet.Cells[row, 19].Value = FormatDecimal(item.AllEnergy);
                    worksheet.Cells[row, 20].Value = FormatDecimal(item.ReactiveEnergySum);
                    worksheet.Cells[row, 21].Value = FormatDecimal(item.Freq);
                    row++;
                }

                // Автоподбор ширины столбцов
                worksheet.Cells.AutoFitColumns();

                // Сохраняем файл
                var fileBytes = package.GetAsByteArray();
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
            }

            // Сохраняем информацию в базу данных
            var report = new Report
            {
                Type = request.Type,
                Name = request.Name,
                Path = fileName,
                Size = new FileInfo(filePath).Length,
                CreatedAt = DateTime.Now,
                CreatedByUserId = request.CreatedByUserId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Получаем пользователя для ответа
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.CreatedByUserId);

            var response = new ReportResponse
            {
                Id = report.Id,
                Type = report.Type,
                Name = report.Name,
                Size = report.Size,
                Path = report.Path,
                CreatedAt = report.CreatedAt ?? DateTime.Now,
                CreatedByUserName = user?.Name ?? "Неизвестный пользователь",
                CreatedByUserSurname = user?.Surname ?? ""
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при создании отчета", details = ex.Message });
        }
    }

    [HttpPost("create-advanced")]
    public async Task<ActionResult<ReportResponse>> CreateAdvancedReport([FromBody] CreateAdvancedReportRequest request)
    {
        try
        {
            // Валидация входных параметров
            if (request == null)
            {
                return BadRequest("Запрос не может быть пустым");
            }
            
            if (!request.MeterIds.Any() && !request.ObjectIds.Any())
            {
                return BadRequest("Необходимо выбрать хотя бы один счетчик или объект");
            }
            
            if (!request.Parameters.Any())
            {
                return BadRequest("Необходимо выбрать хотя бы один параметр для отчета");
            }
            
            if (request.DateFrom >= request.DateTo)
            {
                return BadRequest("Дата начала должна быть меньше даты окончания");
            }
            
            Console.WriteLine($"=== Создание расширенного отчета ===");
            Console.WriteLine($"Тип: {request.Type}");
            Console.WriteLine($"Название: {request.Name}");
            Console.WriteLine($"Параметры: {string.Join(", ", request.Parameters)}");
            Console.WriteLine($"Счетчики: {string.Join(", ", request.MeterIds)}");
            Console.WriteLine($"Объекты: {string.Join(", ", request.ObjectIds)}");
            Console.WriteLine($"Период: {request.DateFrom:dd.MM.yyyy HH:mm} - {request.DateTo:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"================================");

            // Создаем Excel файл
            var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var reportsPath = Path.Combine(_environment.ContentRootPath, "Reports");
            
            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }

            var filePath = Path.Combine(reportsPath, fileName);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Данные");
                
                // Добавляем заголовки
                worksheet.Cells[1, 1].Value = "Дата и время";
                worksheet.Cells[1, 2].Value = "Объект";
                worksheet.Cells[1, 3].Value = "Счетчик";
                worksheet.Cells[1, 4].Value = "Тип";
                
                int colIndex = 5;
                foreach (var param in request.Parameters)
                {
                    worksheet.Cells[1, colIndex].Value = param;
                    colIndex++;
                }

                // Стилизуем заголовки
                using (var range = worksheet.Cells[1, 1, 1, colIndex - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Получаем данные по выбранным счетчикам и объектам
                var allData = new List<ReportDataRow>();
                
                // Получаем данные электрических счетчиков
                if (request.MeterIds.Any())
                {
                    // Проверяем, что DeviceId действительно уникальны
                    var uniqueMeterIds = request.MeterIds.Distinct().ToArray();
                    Console.WriteLine($"Запрошенные MeterIds: {string.Join(", ", uniqueMeterIds)}");
                    
                    var electricityQuery = _context.ElectricityDeviceData
                        .Include(ed => ed.Device)
                        .ThenInclude(d => d.Parent)
                        .Where(ed => uniqueMeterIds.Contains(ed.DeviceId))
                        .Where(ed => ed.TimeReading >= request.DateFrom && ed.TimeReading <= request.DateTo);

                    // Если выбраны конкретные объекты, фильтруем по ним
                    if (request.ObjectIds.Any())
                    {
                        electricityQuery = electricityQuery.Where(ed => request.ObjectIds.Contains(ed.Device.ParentId));
                    }

                    var electricityData = await electricityQuery
                        .OrderBy(ed => ed.TimeReading)
                        .ThenBy(ed => ed.DeviceId) // Добавляем сортировку по DeviceId для стабильности
                        .ToListAsync();

                    Console.WriteLine($"Найдено записей электрических счетчиков: {electricityData.Count}");
                    Console.WriteLine($"Уникальных DeviceId в данных: {electricityData.Select(ed => ed.DeviceId).Distinct().Count()}");

                    foreach (var item in electricityData)
                    {
                        var dataRow = new ReportDataRow
                        {
                            Timestamp = item.TimeReading,
                            ObjectName = item.Device.Parent?.Name ?? "Неизвестный объект",
                            MeterName = item.Device.Name ?? "Неизвестный счетчик",
                            MeterType = "Электрический",
                            DeviceId = item.DeviceId, // Добавляем DeviceId для уникальной идентификации
                            Values = new Dictionary<string, decimal>()
                        };

                        // Добавляем значения параметров
                        foreach (var param in request.Parameters)
                        {
                            var value = GetParameterValue(item, param);
                            dataRow.Values[param] = value;
                        }

                        allData.Add(dataRow);
                    }
                }
                else if (request.ObjectIds.Any())
                {
                    // Если не выбраны счетчики, но выбраны объекты - получаем все счетчики этих объектов
                    var uniqueObjectIds = request.ObjectIds.Distinct().ToArray();
                    Console.WriteLine($"Запрошенные ObjectIds: {string.Join(", ", uniqueObjectIds)}");
                    
                    var electricityQuery = _context.ElectricityDeviceData
                        .Include(ed => ed.Device)
                        .ThenInclude(d => d.Parent)
                        .Where(ed => uniqueObjectIds.Contains(ed.Device.ParentId))
                        .Where(ed => ed.TimeReading >= request.DateFrom && ed.TimeReading <= request.DateTo);

                    var electricityData = await electricityQuery
                        .OrderBy(ed => ed.TimeReading)
                        .ThenBy(ed => ed.DeviceId) // Добавляем сортировку по DeviceId
                        .ToListAsync();

                    Console.WriteLine($"Найдено записей электрических счетчиков по объектам: {electricityData.Count}");
                    Console.WriteLine($"Уникальных DeviceId в данных: {electricityData.Select(ed => ed.DeviceId).Distinct().Count()}");

                    foreach (var item in electricityData)
                    {
                        var dataRow = new ReportDataRow
                        {
                            Timestamp = item.TimeReading,
                            ObjectName = item.Device.Parent?.Name ?? "Неизвестный объект",
                            MeterName = item.Device.Name ?? "Неизвестный счетчик",
                            MeterType = "Электрический",
                            DeviceId = item.DeviceId, // Добавляем DeviceId
                            Values = new Dictionary<string, decimal>()
                        };

                        // Добавляем значения параметров
                        foreach (var param in request.Parameters)
                        {
                            var value = GetParameterValue(item, param);
                            dataRow.Values[param] = value;
                        }

                        allData.Add(dataRow);
                    }
                }

                // Получаем данные газовых счетчиков
                if (request.MeterIds.Any())
                {
                    // Проверяем, что DeviceId действительно уникальны
                    var uniqueMeterIds = request.MeterIds.Distinct().ToArray();
                    
                    var gasQuery = _context.GasDeviceData
                        .Include(gd => gd.Device)
                        .ThenInclude(d => d.Parent)
                        .Where(gd => uniqueMeterIds.Contains(gd.DeviceId))
                        .Where(gd => gd.ReadingTime >= request.DateFrom && gd.ReadingTime <= request.DateTo);

                    // Если выбраны конкретные объекты, фильтруем по ним
                    if (request.ObjectIds.Any())
                    {
                        gasQuery = gasQuery.Where(gd => request.ObjectIds.Contains(gd.Device.ParentId));
                    }

                    var gasData = await gasQuery
                        .OrderBy(gd => gd.ReadingTime)
                        .ThenBy(gd => gd.DeviceId) // Добавляем сортировку по DeviceId
                        .ToListAsync();

                    Console.WriteLine($"Найдено записей газовых счетчиков: {gasData.Count}");
                    Console.WriteLine($"Уникальных DeviceId в данных: {gasData.Select(gd => gd.DeviceId).Distinct().Count()}");

                    foreach (var item in gasData)
                    {
                        var dataRow = new ReportDataRow
                        {
                            Timestamp = item.ReadingTime,
                            ObjectName = item.Device.Parent?.Name ?? "Неизвестный объект",
                            MeterName = item.Device.Name ?? "Неизвестный счетчик",
                            MeterType = "Газовый",
                            DeviceId = item.DeviceId, // Добавляем DeviceId
                            Values = new Dictionary<string, decimal>()
                        };

                        // Добавляем значения параметров
                        foreach (var param in request.Parameters)
                        {
                            var value = GetGasParameterValue(item, param);
                            dataRow.Values[param] = value;
                        }

                        allData.Add(dataRow);
                    }
                }
                else if (request.ObjectIds.Any())
                {
                    // Если не выбраны счетчики, но выбраны объекты - получаем все счетчики этих объектов
                    var uniqueObjectIds = request.ObjectIds.Distinct().ToArray();
                    
                    var gasQuery = _context.GasDeviceData
                        .Include(gd => gd.Device)
                        .ThenInclude(d => d.Parent)
                        .Where(gd => uniqueObjectIds.Contains(gd.Device.ParentId))
                        .Where(gd => gd.ReadingTime >= request.DateFrom && gd.ReadingTime <= request.DateTo);

                    var gasData = await gasQuery
                        .OrderBy(gd => gd.ReadingTime)
                        .ThenBy(gd => gd.DeviceId) // Добавляем сортировку по DeviceId
                        .ToListAsync();

                    Console.WriteLine($"Найдено записей газовых счетчиков по объектам: {gasData.Count}");
                    Console.WriteLine($"Уникальных DeviceId в данных: {gasData.Select(gd => gd.DeviceId).Distinct().Count()}");

                    foreach (var item in gasData)
                    {
                        var dataRow = new ReportDataRow
                        {
                            Timestamp = item.ReadingTime,
                            ObjectName = item.Device.Parent?.Name ?? "Неизвестный объект",
                            MeterName = item.Device.Name ?? "Неизвестный счетчик",
                            MeterType = "Газовый",
                            DeviceId = item.DeviceId, // Добавляем DeviceId
                            Values = new Dictionary<string, decimal>()
                        };

                        // Добавляем значения параметров
                        foreach (var param in request.Parameters)
                        {
                            var value = GetGasParameterValue(item, param);
                            dataRow.Values[param] = value;
                        }

                        allData.Add(dataRow);
                    }
                }

                // Сортируем все данные по времени, затем по DeviceId для стабильности
                allData = allData.OrderBy(d => d.Timestamp).ThenBy(d => d.DeviceId).ToList();

                // Логируем для отладки
                Console.WriteLine($"Всего записей: {allData.Count}");
                Console.WriteLine($"Уникальных DeviceId: {allData.Select(d => d.DeviceId).Distinct().Count()}");
                Console.WriteLine($"Уникальных временных меток: {allData.Select(d => d.Timestamp).Distinct().Count()}");
                
                // Проверяем на дублирование
                var duplicates = allData.GroupBy(d => new { d.Timestamp, d.DeviceId })
                    .Where(g => g.Count() > 1)
                    .Select(g => new { g.Key.Timestamp, g.Key.DeviceId, Count = g.Count() })
                    .ToList();
                
                if (duplicates.Any())
                {
                    Console.WriteLine($"Найдены дублирующиеся записи:");
                    foreach (var dup in duplicates)
                    {
                        Console.WriteLine($"  Время: {dup.Timestamp}, DeviceId: {dup.DeviceId}, Количество: {dup.Count}");
                    }
                    
                    // Убираем дубликаты, оставляя только первую запись для каждой комбинации время+DeviceId
                    allData = allData.GroupBy(d => new { d.Timestamp, d.DeviceId })
                        .Select(g => g.First())
                        .OrderBy(d => d.Timestamp).ThenBy(d => d.DeviceId)
                        .ToList();
                    
                    Console.WriteLine($"После удаления дубликатов записей: {allData.Count}");
                }
                
                // Дополнительная проверка: убеждаемся, что для каждого времени у нас есть данные от каждого счетчика
                var timeGroups = allData.GroupBy(d => d.Timestamp).ToList();
                foreach (var timeGroup in timeGroups)
                {
                    var deviceIdsInTime = timeGroup.Select(d => d.DeviceId).Distinct().ToList();
                    var expectedDeviceIds = allData.Select(d => d.DeviceId).Distinct().ToList();
                    
                    if (deviceIdsInTime.Count != expectedDeviceIds.Count)
                    {
                        Console.WriteLine($"⚠️ Время {timeGroup.Key}: найдено {deviceIdsInTime.Count} счетчиков из {expectedDeviceIds.Count} ожидаемых");
                        var missingDevices = expectedDeviceIds.Except(deviceIdsInTime).ToList();
                        if (missingDevices.Any())
                        {
                            Console.WriteLine($"  Отсутствуют DeviceId: {string.Join(", ", missingDevices)}");
                        }
                    }
                }

                // Заполняем Excel данными
                int row = 2;
                foreach (var dataRow in allData)
                {
                    worksheet.Cells[row, 1].Value = dataRow.Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
                    worksheet.Cells[row, 2].Value = dataRow.ObjectName;
                    worksheet.Cells[row, 3].Value = dataRow.MeterName;
                    worksheet.Cells[row, 4].Value = dataRow.MeterType;
                    
                    colIndex = 5;
                    foreach (var param in request.Parameters)
                    {
                        var value = dataRow.Values.ContainsKey(param) ? dataRow.Values[param] : 0;
                        worksheet.Cells[row, colIndex].Value = FormatDecimal(value);
                        colIndex++;
                    }
                    row++;
                }

                // Автоподбор ширины столбцов
                worksheet.Cells.AutoFitColumns();

                // Сохраняем файл
                var fileBytes = package.GetAsByteArray();
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
            }

            // Сохраняем информацию в базу данных
            var report = new Report
            {
                Type = request.Type,
                Name = request.Name,
                Path = fileName,
                Size = new FileInfo(filePath).Length,
                CreatedAt = DateTime.Now,
                CreatedByUserId = request.CreatedByUserId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Получаем пользователя для ответа
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.CreatedByUserId);

            var response = new ReportResponse
            {
                Id = report.Id,
                Type = report.Type,
                Name = report.Name,
                Size = report.Size,
                Path = report.Path,
                CreatedAt = report.CreatedAt ?? DateTime.Now,
                CreatedByUserName = user?.Name ?? "Неизвестный пользователь",
                CreatedByUserSurname = user?.Surname ?? ""
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при создании отчета", details = ex.Message });
        }
    }

    private decimal GetParameterValue(ElectricityDeviceDatum item, string parameter)
    {
        return parameter switch
        {
            "UL1N" => item.UL1N,
            "UL2N" => item.UL2N,
            "UL3N" => item.UL3N,
            "UL1L2" => item.UL1L2,
            "UL2L3" => item.UL2L3,
            "UL3L1" => item.UL3L1,
            "IL1" => item.IL1,
            "IL2" => item.IL2,
            "IL3" => item.IL3,
            "PL1" => item.PL1,
            "PL2" => item.PL2,
            "PL3" => item.PL3,
            "PSum" => item.PSum,
            "QL1" => item.QL1,
            "QL2" => item.QL2,
            "QL3" => item.QL3,
            "QSum" => item.QSum,
            "AllEnergy" => item.AllEnergy,
            "ReactiveEnergySum" => item.ReactiveEnergySum,
            "Freq" => item.Freq,
            _ => 0
        };
    }

    private decimal GetGasParameterValue(GasDeviceDatum item, string parameter)
    {
        return parameter switch
        {
            "TemperatureGas" => item.TemperatureGas,
            "WorkingVolume" => item.WorkingVolume,
            "StandardVolume" => item.StandardVolume,
            "InstantaneousFlow" => item.InstantaneousFlow,
            "BatteryLive" => item.BatteryLive ?? 0,
            "PressureGas" => item.PressureGas ?? 0,
            "Power" => item.Power ?? 0,
            _ => 0
        };
    }

    [HttpPost("create-visualization")]
    public async Task<ActionResult<ReportResponse>> CreateVisualizationReport([FromBody] CreateVisualizationReportRequest request)
    {
        try
        {
            // Валидация входных параметров
            if (request == null)
            {
                return BadRequest("Запрос не может быть пустым");
            }
            
            if (!request.Data.Any())
            {
                return BadRequest("Нет данных для создания отчета");
            }
            
            if (!request.Parameters.Any())
            {
                return BadRequest("Необходимо выбрать хотя бы один параметр для отчета");
            }

            Console.WriteLine($"=== Создание отчета визуализации ===");
            Console.WriteLine($"Тип: {request.Type}");
            Console.WriteLine($"Название: {request.Name}");
            Console.WriteLine($"Параметры: {string.Join(", ", request.Parameters)}");
            Console.WriteLine($"Количество записей: {request.Data.Count}");
            Console.WriteLine($"================================");

            // Создаем Excel файл
            var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var reportsPath = Path.Combine(_environment.ContentRootPath, "Reports");
            
            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }

            var filePath = Path.Combine(reportsPath, fileName);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Данные");
                
                // Получаем уникальные ID устройств (как на фронтенде)
                var deviceIds = request.Data.Select(item => item.DeviceId).Distinct().ToList();
                
                // Создаем колонки точно как на фронтенде
                var columns = new List<object>();
                columns.Add("Дата");
                columns.Add("Время");
                
                // Для каждого параметра создаем колонку для каждого устройства
                foreach (var paramKey in request.Parameters)
                {
                    foreach (var deviceId in deviceIds)
                    {
                        var deviceName = request.Data.FirstOrDefault(item => item.DeviceId == deviceId)?.DeviceName ?? $"Device {deviceId}";
                        columns.Add($"{paramKey} - {deviceName}");
                    }
                }

                // Добавляем заголовки
                for (int i = 0; i < columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = columns[i];
                }

                // Стилизуем заголовки
                using (var range = worksheet.Cells[1, 1, 1, columns.Count])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Группируем данные по времени (как на фронтенде)
                var timeGroups = new Dictionary<DateTime, Dictionary<long, VisualizationDataRow>>();
                foreach (var item in request.Data)
                {
                    var timeKey = item.Timestamp;
                    if (!timeGroups.ContainsKey(timeKey))
                    {
                        timeGroups[timeKey] = new Dictionary<long, VisualizationDataRow>();
                    }
                    timeGroups[timeKey][item.DeviceId] = item;
                }

                // Получаем отсортированные временные метки
                var sortedTimestamps = timeGroups.Keys.OrderBy(t => t).ToList();

                Console.WriteLine($"Уникальных DeviceId: {deviceIds.Count}");
                Console.WriteLine($"Уникальных временных меток: {sortedTimestamps.Count}");
                Console.WriteLine($"Количество колонок: {columns.Count}");

                // Заполняем данные по строкам (как на фронтенде)
                int row = 2;
                foreach (var timestamp in sortedTimestamps)
                {
                    var timeGroup = timeGroups[timestamp];
                    
                    // Конвертируем UTC в локальное время (UTC+3 для Москвы)
                    var localTime = timestamp.AddHours(3);
                    
                    // Колонка 1: Дата
                    worksheet.Cells[row, 1].Value = localTime.ToString("dd.MM.yyyy");
                    // Колонка 2: Время
                    worksheet.Cells[row, 2].Value = localTime.ToString("HH:mm:ss");
                    
                    // Колонки 3+: Параметры для каждого устройства
                    int colIndex = 3;
                    foreach (var paramKey in request.Parameters)
                    {
                        foreach (var deviceId in deviceIds)
                        {
                            // Находим данные для конкретного устройства и параметра
                            if (timeGroup.ContainsKey(deviceId) && 
                                timeGroup[deviceId].Values.ContainsKey(paramKey))
                            {
                                var value = timeGroup[deviceId].Values[paramKey];
                                worksheet.Cells[row, colIndex].Value = FormatDecimal((decimal)value);
                            }
                            else
                            {
                                worksheet.Cells[row, colIndex].Value = FormatDecimal(0);
                            }
                            colIndex++;
                        }
                    }
                    row++;
                }

                // Автоподбор ширины столбцов
                worksheet.Cells.AutoFitColumns();

                // Сохраняем файл
                var fileBytes = package.GetAsByteArray();
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
            }

            // Сохраняем информацию в базу данных
            var report = new Report
            {
                Type = request.Type,
                Name = request.Name,
                Path = fileName,
                Size = new FileInfo(filePath).Length,
                CreatedAt = DateTime.Now,
                CreatedByUserId = request.CreatedByUserId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Получаем пользователя для ответа
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.CreatedByUserId);

            var response = new ReportResponse
            {
                Id = report.Id,
                Type = report.Type,
                Name = report.Name,
                Size = report.Size,
                Path = report.Path,
                CreatedAt = report.CreatedAt ?? DateTime.Now,
                CreatedByUserName = user?.Name ?? "Неизвестный пользователь",
                CreatedByUserSurname = user?.Surname ?? ""
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при создании отчета", details = ex.Message });
        }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadReport(long id)
    {
        try
        {
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null)
            {
                return NotFound("Отчет не найден");
            }

            var filePath = Path.Combine(_environment.ContentRootPath, "Reports", report.Path);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Файл отчета не найден");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", report.Name + ".xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при скачивании отчета", details = ex.Message });
        }
    }
}

// Вспомогательные классы для отчета
public class ReportDataRow
{
    public DateTime Timestamp { get; set; }
    public string ObjectName { get; set; } = "";
    public string MeterName { get; set; } = "";
    public string MeterType { get; set; } = "";
    public long DeviceId { get; set; } // Добавляем DeviceId для уникальной идентификации
    public Dictionary<string, decimal> Values { get; set; } = new();
} 