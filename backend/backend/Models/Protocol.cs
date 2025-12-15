using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Протоколы связи
/// </summary>
public partial class Protocol
{
    public long Id { get; set; }

    /// <summary>
    /// Название протокола.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Код протокола. По нему осуществляется идентификация протокола
    /// </summary>
    public string? CodeName { get; set; }
}
