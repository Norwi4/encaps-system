using System.Text.Json;
using System.Reflection;

namespace backend.Helpers;

public static class PlateInfoHelper
{
    // Маппинг между названиями колонок в БД и названиями свойств в C#
    private static readonly Dictionary<string, string> ColumnToPropertyMap = new()
    {
        // Electrical device mappings
        { "u_l1_n", "UL1N" },
        { "u_l2_n", "UL2N" },
        { "u_l3_n", "UL3N" },
        { "u_l1_l2", "UL1L2" },
        { "u_l2_l3", "UL2L3" },
        { "u_l3_l1", "UL3L1" },
        { "i_l1", "IL1" },
        { "i_l2", "IL2" },
        { "i_l3", "IL3" },
        { "p_l1", "PL1" },
        { "p_l2", "PL2" },
        { "p_l3", "PL3" },
        { "p_sum", "PSum" },
        { "q_l1", "QL1" },
        { "q_l2", "QL2" },
        { "q_l3", "QL3" },
        { "q_sum", "QSum" },
        { "all_energy", "AllEnergy" },
        { "reactive_energy_sum", "ReactiveEnergySum" },
        { "freq", "Freq" },
        { "aq1", "Aq1" },
        { "aq2", "Aq2" },
        { "aq3", "Aq3" },
        { "fund_pf_cf1", "FundPfCf1" },
        { "fund_pf_cf2", "FundPfCf2" },
        { "fund_pf_cf3", "FundPfCf3" },
        { "rotation_field", "RotationField" },
        { "rqc_l1", "RqcL1" },
        { "rqc_l2", "RqcL2" },
        { "rqc_l3", "RqcL3" },
        { "rqd_l1", "RqdL1" },
        { "rqd_l2", "RqdL2" },
        { "rqd_l3", "RqdL3" },
        { "react_qi_l1", "ReactQIL1" },
        { "react_qi_l2", "ReactQIL2" },
        { "react_qi_l3", "ReactQIL3" },
        { "react_qc_l1", "ReactQCL1" },
        { "react_qc_l2", "ReactQCL2" },
        { "react_qc_l3", "ReactQCL3" },
        { "h_u_l1", "HUL1" },
        { "h_u_l2", "HUL2" },
        { "h_u_l3", "HUL3" },
        { "h_i_l1", "HIL1" },
        { "h_i_l2", "HIL2" },
        { "h_i_l3", "HIL3" },
        { "angle1", "Angle1" },
        { "angle2", "Angle2" },
        { "angle3", "Angle3" },
        { "all_energy_k", "AllEnergyK" },
        
        
        // Gas device mappings
        { "working_volume", "WorkingVolume" },
        { "standard_volume", "StandardVolume" },
        { "pressure_gas", "PressureGas" },
        { "temperature_gas", "TemperatureGas" },
        { "power", "Power" },
        { "time_reading", "ReadingTime" }
    };

    public static Dictionary<string, PlateInfoField>? ParsePlateInfo(string? plateInfoJson)
    {
        if (string.IsNullOrEmpty(plateInfoJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, PlateInfoField>>(plateInfoJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static List<string> GetFilteredParameters(Dictionary<string, PlateInfoField>? plateInfo)
    {
        if (plateInfo == null)
            return new List<string>();

        return plateInfo.Keys.ToList();
    }

    public static string GetPropertyName(string columnName)
    {
        return ColumnToPropertyMap.TryGetValue(columnName, out var propertyName) ? propertyName : columnName;
    }

    public static PropertyInfo? GetPropertyInfo<T>(string columnName)
    {
        var propertyName = GetPropertyName(columnName);
        return typeof(T).GetProperty(propertyName);
    }
}

public class PlateInfoField
{
    public string Label { get; set; } = string.Empty;
    public string Digit { get; set; } = "2";
}
