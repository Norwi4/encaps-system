namespace backend.Models;

public class RabbitMQSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string QueueReceive { get; set; } = string.Empty;
    public string Consumer { get; set; } = string.Empty;
    public string InputQueue { get; set; } = string.Empty;
} 