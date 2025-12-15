namespace backend.Contracts
{
    public class ObjectWithDevicesResponse
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Place { get; set; }
        public string? Comment { get; set; }
        public List<DeviceShortResponse> Devices { get; set; } = new();
    }

    public class DeviceShortResponse
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? SerialNo { get; set; }
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? Channel { get; set; }
        public DateOnly? InstallDate { get; set; }
        public string? Comment { get; set; }
        public DateTime? TrustedBefore { get; set; }
    }
} 