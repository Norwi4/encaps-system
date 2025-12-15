using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Event
{
    public long Id { get; set; }

    public long? ObjectId { get; set; }

    public long? DeviceId { get; set; }

    public long? EventId { get; set; }

    public DateTime? Date { get; set; }

    public bool? Status { get; set; }

    public string? Description { get; set; }

    public virtual Event1? EventNavigation { get; set; }
    public virtual Object? Object { get; set; }
    public virtual Device? Device { get; set; }
}
