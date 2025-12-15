using Microsoft.AspNetCore.SignalR;
using backend.Hubs;
using backend.Contracts;

namespace backend.Services;

public interface INotificationService
{
    Task SendUserActionNotification(UserActionsLogResponse userAction);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendUserActionNotification(UserActionsLogResponse userAction)
    {
        await _hubContext.Clients.Group("notifications").SendAsync("ReceiveUserAction", userAction);
    }
} 