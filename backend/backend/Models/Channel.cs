using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Каналы связи
/// </summary>
public partial class Channel
{
    public long Id { get; set; }

    /// <summary>
    /// Канал, через который ведется связь
    /// </summary>
    public string? Dev { get; set; }

    public int BaudRate { get; set; }

    public int StopBits { get; set; }

    public int DataBits { get; set; }

    public int OpenTimeout { get; set; }

    /// <summary>
    /// Количество попыток соединения
    /// </summary>
    public int OpenAttempts { get; set; }

    public string? Comment { get; set; }

    public string? Ip { get; set; }

    public int? Port { get; set; }

    public bool? CloseAfterSession { get; set; }

    public int? TypeLink { get; set; }

    /// <summary>
    /// Время отключения при бездействии (для удерживаемых соединений)
    /// </summary>
    public int? TimeDisconnect { get; set; }

    /// <summary>
    /// Наименование канала
    /// </summary>
    public string? Name { get; set; }

    public char Parity { get; set; }

    /// <summary>
    /// Данные поверх TCP соединения без изменений
    /// </summary>
    public bool? OverTcp { get; set; }
}
