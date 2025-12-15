namespace backend.Contracts
{
    public class DeviceEditRequest
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public DateTime? TrustedBefore { get; set; }
        public string? IpAddress { get; set; }
        public int? NetworkPort { get; set; }
        public double? KoeffTrans { get; set; }
        public long? ScanInterval { get; set; }
        public int? SortId { get; set; }
        public int? DevAddr { get; set; }
        public bool? Active { get; set; }
        public long? UserId { get; set; }
    }

    public class DeviceEditResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DeviceDetails? Device { get; set; }
    }

    public class DeviceDetails
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public DateTime? TrustedBefore { get; set; }
        public string? IpAddress { get; set; }
        public int? NetworkPort { get; set; }
        public double? KoeffTrans { get; set; }
        public long? ScanInterval { get; set; }
        public long ChannelId { get; set; }
        public string? ChannelName { get; set; }
        public bool Active { get; set; }
        public string? SerialNo { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? LastReceive { get; set; }
        public int? SortId { get; set; }
        public int? DevAddr { get; set; }
    }
}
