using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Пользователи и права
/// </summary>
public partial class User
{
    public long Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    /// <summary>
    /// ФИО
    /// </summary>
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Comment { get; set; }

    public string? Surname { get; set; }

    public string? Patronymic { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<UserAction> UserActions { get; set; } = new List<UserAction>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
