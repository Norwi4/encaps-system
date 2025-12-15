using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Action
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Name { get; set; }
}
