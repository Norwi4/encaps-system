using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class DeviceType
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;
}
