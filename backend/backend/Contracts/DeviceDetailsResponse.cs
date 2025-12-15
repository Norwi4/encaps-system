using backend.Controllers;

namespace backend.Contracts;

public class DeviceDetailsResponse
{
    public long DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? ObjectName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastReading { get; set; }
    public List<DeviceDetailParam> Parameters { get; set; } = new();
}