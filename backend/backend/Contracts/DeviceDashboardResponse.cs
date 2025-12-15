namespace backend.Contracts
{
    public class DeviceDashboardResponse
    {
        public List<DeviceDashboardObject> Objects { get; set; } = new();
    }

    public class DeviceDashboardObject
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public List<DeviceDashboardDevice> Devices { get; set; } = new();
    }

    public class DeviceDashboardDevice
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? StatusColor { get; set; } // red, blue, green
        public int? SortId { get; set; }
        public List<DeviceDashboardParam> Params { get; set; } = new();
    }

    public class DeviceDashboardParam
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
} 