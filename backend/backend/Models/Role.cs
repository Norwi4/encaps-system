using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Справочник - роли доступа 
/// </summary>
public partial class Role
{
    public long Id { get; set; }

    /// <summary>
    /// Роль, описание
    /// </summary>
    public List<string> RoleName { get; set; } = null!;

    /// <summary>
    /// Код роли, по нему связываются подчиненные таблицы
    /// </summary>
    public long RoleCode { get; set; }
}
