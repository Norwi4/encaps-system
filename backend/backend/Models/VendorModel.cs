using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Модели устройств с привязкой в вендору
/// </summary>
public partial class VendorModel
{
    public long Id { get; set; }

    public long VendorId { get; set; }

    /// <summary>
    /// Наименование модели
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// JSON с описанием выводимых полей для конкретного устройства
    /// </summary>
    public string? PlateInfo { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;
}
