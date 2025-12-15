using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class GasDeviceReference
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public decimal SubstitutionPressure { get; set; }

    public decimal SubstitutionCompressibility { get; set; }

    public decimal CorrectionFactor { get; set; }

    public virtual Device Device { get; set; } = null!;
}
