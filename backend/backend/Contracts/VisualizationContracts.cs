using System;
using System.Collections.Generic;

namespace backend.Contracts
{
    public class TimeInterval
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class VisualizationDataResponse
    {
        public List<VisualizationDataPoint> Data { get; set; } = new List<VisualizationDataPoint>();
        public string Period { get; set; } = "";
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string MeterType { get; set; } = "";
        public long[] ObjectIds { get; set; } = new long[0];
        public long[] MeterIds { get; set; } = new long[0];
        public string[] Parameters { get; set; } = new string[0];
    }

    public class VisualizationDataPoint
    {
        public DateTime Timestamp { get; set; }
        public long DeviceId { get; set; }
        public string DeviceName { get; set; } = "";
        public string ObjectName { get; set; } = "";
        public Dictionary<string, decimal> Values { get; set; } = new Dictionary<string, decimal>();
    }

    public class VisualizationObject
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public List<VisualizationDevice> Devices { get; set; } = new List<VisualizationDevice>();
    }

    public class VisualizationDevice
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public int? SortId { get; set; }
    }

    public class VisualizationParameter
    {
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
        public string[] Parameters { get; set; } = new string[0];
    }

    public class VisualizationParameterReadable
    {
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
        public List<VisualizationParameterItem> Parameters { get; set; } = new List<VisualizationParameterItem>();
    }

    public class VisualizationParameterItem
    {
        public string Code { get; set; } = "";
        public string FullName { get; set; } = "";
        public string ShortName { get; set; } = "";
    }

    // Обновленные контракты для данных о расходе по площадкам
    public class ConsumptionDataResponse
    {
        public decimal ElectricityConsumption { get; set; }
        public decimal GasConsumption { get; set; }
        public string Period { get; set; } = "";
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class SiteConsumptionData
    {
        public string SiteName { get; set; } = "";
        public decimal ElectricityConsumption { get; set; }
        public decimal GasConsumption { get; set; }
    }

    public class DashboardMetricsResponse
    {
        public List<SiteConsumptionData> PreviousMonthConsumption { get; set; } = new List<SiteConsumptionData>();
        public List<SiteConsumptionData> MonthlyConsumption { get; set; } = new List<SiteConsumptionData>();
        public List<SiteConsumptionData> DailyConsumption { get; set; } = new List<SiteConsumptionData>();
        public List<SiteConsumptionData> PreviousDayConsumption { get; set; } = new List<SiteConsumptionData>();
    }

    // Класс для результата хранимой функции _electro_get_energy_interval
    public class EnergyIntervalData
    {
        public decimal AllEnergyCurrent { get; set; }
        public decimal AllEnergyLast { get; set; }
    }
} 