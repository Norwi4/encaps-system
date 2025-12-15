using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using backend.Models;

namespace backend.Services;

public interface IRabbitMQService
{
    void SendMessage(string eventCode, object eventValue);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService(IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value;
        
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Объявляем очередь
        _channel.QueueDeclare(
            queue: _settings.InputQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public void SendMessage(string eventCode, object eventValue)
    {
        var message = new Dictionary<string, object>
        {
            { eventCode, eventValue }
        };

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: "",
            routingKey: _settings.InputQueue,
            basicProperties: null,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 