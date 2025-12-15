using System;

namespace backend.Contracts;

public class UserActionsLogRequest
{
    public long? UserId { get; set; }
    public long? ActionId { get; set; }
    public string PeriodType { get; set; } = "new"; // new, week, month, custom
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UserActionsLogResponse
{
    public long Id { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserSurname { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UserActionsLogListResponse
{
    public List<UserActionsLogResponse> UserActions { get; set; } = new();
    public int TotalCount { get; set; }
}

public class CreateUserActionRequest
{
    public long UserId { get; set; }
    public long ActionId { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateDeviceRequest
{
    public string? Comment { get; set; }
    public DateTime? TrustedBefore { get; set; }
} 