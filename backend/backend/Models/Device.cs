using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Устройства с привязкой к объекту
/// </summary>
public partial class Device
{
    public long Id { get; set; }

    public long ParentId { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Производитель
    /// </summary>
    public long? Vendor { get; set; }

    /// <summary>
    /// Модель
    /// </summary>
    public long? Model { get; set; }

    /// <summary>
    /// Серийный номер
    /// </summary>
    public string? SerialNo { get; set; }

    /// <summary>
    /// Путь и название файла изображения
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Дата действительности поверки
    /// </summary>
    public DateTime? TrustedBefore { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// ID канала передачи данных (channel)
    /// </summary>
    public long ChannelId { get; set; }

    /// <summary>
    /// ID типа устройства (electrical, gas, water)
    /// </summary>
    public long? DeviceTypeId { get; set; }

    public bool Active { get; set; }

    /// <summary>
    /// Требуется обновление настроек устройства
    /// </summary>
    public bool RequireRefresh { get; set; }

    /// <summary>
    /// Последний опрос
    /// </summary>
    public DateTime? LastReceive { get; set; }

    public DateOnly? InstallationDate { get; set; }

    /// <summary>
    /// ID для сортировки устройств на дашборде
    /// </summary>
    public int? SortId { get; set; }

    public virtual ICollection<DeviceDatum> DeviceData { get; set; } = new List<DeviceDatum>();

    public virtual ICollection<DeviceSetting> DeviceSettings { get; set; } = new List<DeviceSetting>();

    public virtual ICollection<GasDeviceDatum> GasDeviceData { get; set; } = new List<GasDeviceDatum>();

    public virtual ICollection<GasDeviceReference> GasDeviceReferences { get; set; } = new List<GasDeviceReference>();

    public virtual Object Parent { get; set; } = null!;

    public virtual DeviceType? DeviceType { get; set; }

    public virtual Channel Channel { get; set; } = null!;

    public virtual VendorModel? VendorModel { get; set; }
}
