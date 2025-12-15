using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Таблица производителей
/// </summary>
public partial class Vendor
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<VendorModel> VendorModels { get; set; } = new List<VendorModel>();
}
