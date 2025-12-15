using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Report
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public long Size { get; set; }

    public string Path { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public long? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }
}
