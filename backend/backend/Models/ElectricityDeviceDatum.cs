using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class ElectricityDeviceDatum
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public DateTime TimeReading { get; set; }

    /// <summary>
    /// Напряжение l1_n
    /// </summary>
    public decimal UL1N { get; set; }

    public decimal UL2N { get; set; }

    public decimal UL3N { get; set; }

    public decimal UL1L2 { get; set; }

    public decimal UL2L3 { get; set; }

    public decimal UL3L1 { get; set; }

    /// <summary>
    /// Ток по фазе 1
    /// </summary>
    public decimal IL1 { get; set; }

    public decimal IL2 { get; set; }

    public decimal IL3 { get; set; }

    /// <summary>
    /// Активная мощность по фазе 1
    /// </summary>
    public decimal PL1 { get; set; }

    public decimal PL2 { get; set; }

    public decimal PL3 { get; set; }

    public decimal PSum { get; set; }

    /// <summary>
    /// Реактивная мощность L1
    /// </summary>
    public decimal QL1 { get; set; }

    public decimal QL2 { get; set; }

    public decimal QL3 { get; set; }

    public decimal QSum { get; set; }

    /// <summary>
    /// Active energy SUMM
    /// </summary>
    public decimal AllEnergy { get; set; }

    /// <summary>
    /// Reactive energy SUMM
    /// </summary>
    public decimal ReactiveEnergySum { get; set; }

    public decimal Freq { get; set; }

    /// <summary>
    /// Apparent power L1
    /// </summary>
    public decimal? Aq1 { get; set; }

    /// <summary>
    /// Apparent power L2
    /// </summary>
    public decimal? Aq2 { get; set; }

    /// <summary>
    /// Apparent power L3
    /// </summary>
    public decimal? Aq3 { get; set; }

    /// <summary>
    /// Fund power factor, CosPhi
    /// </summary>
    public decimal? FundPfCf1 { get; set; }

    /// <summary>
    /// Fund power factor, CosPhi
    /// </summary>
    public decimal? FundPfCf2 { get; set; }

    /// <summary>
    /// Fund power factor, CosPhi
    /// </summary>
    public decimal? FundPfCf3 { get; set; }

    /// <summary>
    /// Rotation field
    /// </summary>
    public decimal? RotationField { get; set; }

    /// <summary>
    /// Real energy consumed
    /// </summary>
    public decimal? RqcL1 { get; set; }

    /// <summary>
    /// Real energy consumed
    /// </summary>
    public decimal? RqcL2 { get; set; }

    /// <summary>
    /// Real energy consumed
    /// </summary>
    public decimal? RqcL3 { get; set; }

    /// <summary>
    /// Real energy delivered
    /// </summary>
    public decimal? RqdL1 { get; set; }

    /// <summary>
    /// Real energy delivered
    /// </summary>
    public decimal? RqdL2 { get; set; }

    /// <summary>
    /// Real energy delivered
    /// </summary>
    public decimal? RqdL3 { get; set; }

    /// <summary>
    /// Reaktive energy inductive
    /// </summary>
    public decimal? ReactQIL1 { get; set; }

    /// <summary>
    /// Reaktive energy inductive
    /// </summary>
    public decimal? ReactQIL2 { get; set; }

    /// <summary>
    /// Reaktive energy inductive
    /// </summary>
    public decimal? ReactQIL3 { get; set; }

    /// <summary>
    /// Reaktive energy capacitive
    /// </summary>
    public decimal? ReactQCL1 { get; set; }

    /// <summary>
    /// Reaktive energy capacitive
    /// </summary>
    public decimal? ReactQCL2 { get; set; }

    /// <summary>
    /// Reaktive energy capacitive
    /// </summary>
    public decimal? ReactQCL3 { get; set; }

    /// <summary>
    /// Harmonic THD U
    /// </summary>
    public decimal? HUL1 { get; set; }

    /// <summary>
    /// Harmonic THD U
    /// </summary>
    public decimal? HUL2 { get; set; }

    /// <summary>
    /// Harmonic THD U
    /// </summary>
    public decimal? HUL3 { get; set; }

    /// <summary>
    /// Harmonic THD I
    /// </summary>
    public decimal? HIL1 { get; set; }

    /// <summary>
    /// Harmonic THD I
    /// </summary>
    public decimal? HIL2 { get; set; }

    /// <summary>
    /// Harmonic THD I
    /// </summary>
    public decimal? HIL3 { get; set; }

    /// <summary>
    /// Угол между фазными напряжениями
    /// </summary>
    public decimal? Angle1 { get; set; }

    /// <summary>
    /// Угол между фазными напряжениями
    /// </summary>
    public decimal? Angle2 { get; set; }

    /// <summary>
    /// Угол между фазными напряжениями
    /// </summary>
    public decimal? Angle3 { get; set; }

    /// <summary>
    /// Накопленная энергия с учетом коэффициента трансформации
    /// </summary>
    public decimal? AllEnergyK { get; set; }

    public virtual Device Device { get; set; } = null!;
}
