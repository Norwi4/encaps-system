using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs;

public class NotificationHub : Hub
{
    public async Task JoinNotificationGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "notifications");
    }

    public async Task LeaveNotificationGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "notifications");
    }
} 