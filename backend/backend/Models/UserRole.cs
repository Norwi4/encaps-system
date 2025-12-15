using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Роли пользователя
/// </summary>
public partial class UserRole
{
    public long Id { get; set; }

    public long UserId { get; set; }

    /// <summary>
    /// Роль пользователя из справочника
    /// </summary>
    public long Role { get; set; }

    /// <summary>
    /// Уровень доступа к роли (закрыто, чтение, запись/чтение)
    /// </summary>
    public char AccessLevel { get; set; }

    public virtual User User { get; set; } = null!;
}
