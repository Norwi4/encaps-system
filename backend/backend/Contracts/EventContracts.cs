using System;

namespace backend.Contracts;

public class EventLogRequest
{
    public long? ObjectGroupId { get; set; }
    public string PeriodType { get; set; } = "new"; // new, week, month, custom
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsCritical { get; set; } // null = все события, true = только критические, false = только некритические
}

public class EventLogResponse
{
    public long Id { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string ObjectAddress { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
}

public class EventLogListResponse
{
    public List<EventLogResponse> Events { get; set; } = new();
    public int TotalCount { get; set; }
} 