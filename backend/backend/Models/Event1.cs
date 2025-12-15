using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Event1
{
    public long Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public bool? IsCritical { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
