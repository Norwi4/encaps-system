using backend.Models;
using backend.Hubs;
using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Конфигурация
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// База данных
builder.Services.AddDbContext<BmsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Сервисы
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<IDeviceDataService, DeviceDataService>();

// Background Services
builder.Services.AddHostedService<DeviceDataBackgroundService>();
builder.Services.AddHostedService<MonthlyConsumptionUpdateService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? 
                            new[] { "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");
app.MapHub<NotificationHub>("/notificationHub");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();