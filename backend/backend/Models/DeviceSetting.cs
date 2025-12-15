using System;
using System.Collections.Generic;

namespace backend.Models;

/// <summary>
/// Настройки связи для устройства
/// </summary>
public partial class DeviceSetting
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    /// <summary>
    /// Тип линии связи
    /// 1 - serial
    /// 2 - TCP
    /// 3 - UDP
    /// 4 - modem
    /// </summary>
    public short TypeLink { get; set; }

    /// <summary>
    /// Скорость связи
    /// </summary>
    public long? Speed { get; set; }

    public char? Parity { get; set; }

    public short? DataBit { get; set; }

    public short? StopBit { get; set; }

    /// <summary>
    /// Протокол опроса, ID
    /// </summary>
    public long? ProtocolId { get; set; }

    /// <summary>
    /// Количество повторов сканирования при ошибке опроса
    /// </summary>
    public short? ScanRepeat { get; set; }

    /// <summary>
    /// Интервал между повторными опросами, сек
    /// </summary>
    public short? ScanRepeatInterval { get; set; }

    /// <summary>
    /// Последний опрос
    /// </summary>
    public DateTime? LastSession { get; set; }

    /// <summary>
    /// Статус последнего опроса
    /// </summary>
    public short? StatusLastSession { get; set; }

    /// <summary>
    /// Адрес устройства
    /// </summary>
    public int? DevAddr { get; set; }

    /// <summary>
    /// Адрес начала данных в устройстве
    /// </summary>
    public long? DataAddr { get; set; }

    /// <summary>
    /// Размер данных
    /// </summary>
    public int? DataSize { get; set; }

    /// <summary>
    /// Интервал опроса
    /// </summary>
    public long? ScanInterval { get; set; }

    /// <summary>
    /// Тайм аут ответа устройства
    /// </summary>
    public int? TimeOut { get; set; }

    public string? AuthLogin { get; set; }

    public string? AuthPass { get; set; }

    /// <summary>
    /// Сервисный код для обработчика протоколов. Может использоваться для различных целей обработчиком, здесь можно указать, например, тип опрашиваемых этим устройством данных
    /// </summary>
    public short ProtServiceCode { get; set; }

    /// <summary>
    /// Постобработчик принятых данных. Применяется для финальной обработки принятых данных. Как правило со встроенным протоколом modbus
    /// </summary>
    public string? ProtPostHandlerName { get; set; }

    /// <summary>
    /// Время хранения данных, дней
    /// </summary>
    public int DayDataLive { get; set; }

    /// <summary>
    /// Количество успешных опросов
    /// </summary>
    public long SuccessReceive { get; set; }

    /// <summary>
    /// Количество сбойных опросов
    /// </summary>
    public long BadReceive { get; set; }

    /// <summary>
    /// Коэффициент трансформации
    /// </summary>
    public double KoeffTrans { get; set; }

    public virtual Device Device { get; set; } = null!;
}
