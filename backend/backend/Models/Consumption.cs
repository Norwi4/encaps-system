using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("consumption_by_day")]
public class ConsumptionByDay
{
    [Column("id")]
    public long Id { get; set; }

    [Column("dt")]
    public DateOnly Dt { get; set; }

    [Column("value")]
    public decimal Value { get; set; }

    [Column("device_id")]
    public long DeviceId { get; set; }

    [ForeignKey("DeviceId")]
    public Device? Device { get; set; }
}

[Table("consumption_by_month")]
public class ConsumptionByMonth
{
    [Column("id")]
    public long Id { get; set; }

    [Column("dt")]
    public DateTime Dt { get; set; }

    [Column("value")]
    public decimal Value { get; set; }

    [Column("device_id")]
    public long DeviceId { get; set; }

    [ForeignKey("DeviceId")]
    public Device? Device { get; set; }
}

[Table("consumption_by_today")]
public class ConsumptionByToday
{
    [Column("id")]
    public long Id { get; set; }

    [Column("dt")]
    public DateTime Dt { get; set; }

    [Column("value")]
    public decimal Value { get; set; }

    [Column("device_id")]
    public long DeviceId { get; set; }

    [ForeignKey("DeviceId")]
    public Device? Device { get; set; }
}
