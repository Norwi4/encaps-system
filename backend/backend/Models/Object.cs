using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Объекты, на которых установлены счетчики
/// </summary>
public partial class Object
{
    public long Id { get; set; }

    /// <summary>
    /// Название объекта
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Местоположение
    /// </summary>
    public string? Place { get; set; }

    /// <summary>
    /// Комментарий, произвольный текст
    /// </summary>
    public string? Comment { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
