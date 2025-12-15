using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Данные, полученные с приборов
/// </summary>
public partial class DeviceDatum
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    /// <summary>
    /// Дата / время записи/изменений
    /// </summary>
    public DateTime Dt { get; set; }

    /// <summary>
    /// Тип полученной записи (накопленные данные, мощность и т.д.) - коды из справочника
    /// </summary>
    public long Type { get; set; }

    public float Value { get; set; }

    public string? JsonData { get; set; }

    public virtual Device Device { get; set; } = null!;
}
