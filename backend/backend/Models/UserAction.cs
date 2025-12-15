using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Таблица действий пользователей
/// </summary>
public partial class UserAction
{
    public long Id { get; set; }

    public long ActionId { get; set; }

    public DateTime Date { get; set; }

    public long UserId { get; set; }

    public string Description { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public virtual Action Action { get; set; } = null!;
}
