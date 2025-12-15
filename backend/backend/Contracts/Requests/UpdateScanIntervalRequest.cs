namespace backend.Contracts.Requests;

public class UpdateScanIntervalRequest
{
    public int ScanIntervalMs { get; set; }
    public long? UserId { get; set; }
} 