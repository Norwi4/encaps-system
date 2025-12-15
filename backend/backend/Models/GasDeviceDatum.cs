using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Данные с газовых устройств
/// </summary>
public partial class GasDeviceDatum
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public DateTime ReadingTime { get; set; }

    public decimal TemperatureGas { get; set; }

    public decimal WorkingVolume { get; set; }

    public decimal StandardVolume { get; set; }

    public decimal InstantaneousFlow { get; set; }

    /// <summary>
    /// Осталось времени до исчерпания заряда батареи
    /// </summary>
    public decimal? BatteryLive { get; set; }

    /// <summary>
    /// Давление газа на входе в счетчик
    /// </summary>
    public decimal? PressureGas { get; set; }

    /// <summary>
    /// Мощность
    /// </summary>
    public decimal? Power { get; set; }

    public virtual Device Device { get; set; } = null!;
}
