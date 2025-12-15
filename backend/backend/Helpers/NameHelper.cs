namespace backend.Helpers
{
    public static class NameHelper
    {
        public static string GetParameterFullName(string parameterName)
        {
            return parameterName switch
            {
                "UL1N" => "Напряжение L1-N",
                "UL2N" => "Напряжение L2-N", 
                "UL3N" => "Напряжение L3-N",
                "UL1L2" => "Напряжение L1-L2",
                "UL2L3" => "Напряжение L2-L3",
                "UL3L1" => "Напряжение L3-L1",
                "IL1" => "Ток фазы L1",
                "IL2" => "Ток фазы L2",
                "IL3" => "Ток фазы L3",
                "PL1" => "Активная мощность фазы L1",
                "PL2" => "Активная мощность фазы L2",
                "PL3" => "Активная мощность фазы L3",
                "PSum" => "Суммарная активная мощность",
                "QL1" => "Реактивная мощность фазы L1",
                "QL2" => "Реактивная мощность фазы L2",
                "QL3" => "Реактивная мощность фазы L3",
                "QSum" => "Суммарная реактивная мощность",
                "AllEnergy" => "Активная энергия (сумма)",
                "ReactiveEnergySum" => "Реактивная энергия (сумма)",
                "Freq" => "Частота",
                "Aq1" => "Полная мощность по фазе L1",
                "Aq2" => "Полная мощность по фазе L2",
                "Aq3" => "Полная мощность по фазе L3",
                "AqSum" => "Полная мощность сумма",
                "FundPfCf1" => "Коэффициент мощности фазы L1",
                "FundPfCf2" => "Коэффициент мощности фазы L2",
                "FundPfCf3" => "Коэффициент мощности фазы L3",
                "RotationField" => "Вращающееся поле",
                "RqcL1" => "Реальная потребленная энергия фазы L1",
                "RqcL2" => "Реальная потребленная энергия фазы L2",
                "RqcL3" => "Реальная потребленная энергия фазы L3",
                "RqdL1" => "Реальная отданная энергия фазы L1",
                "RqdL2" => "Реальная отданная энергия фазы L2",
                "RqdL3" => "Реальная отданная энергия фазы L3",
                "ReactQIL1" => "Реактивная индуктивная энергия фазы L1",
                "ReactQIL2" => "Реактивная индуктивная энергия фазы L2",
                "ReactQIL3" => "Реактивная индуктивная энергия фазы L3",
                "ReactQCL1" => "Реактивная емкостная энергия фазы L1",
                "ReactQCL2" => "Реактивная емкостная энергия фазы L2",
                "ReactQCL3" => "Реактивная емкостная энергия фазы L3",
                "HUL1" => "Гармоники THD напряжения фазы L1",
                "HUL2" => "Гармоники THD напряжения фазы L2",
                "HUL3" => "Гармоники THD напряжения фазы L3",
                "HIL1" => "Гармоники THD тока фазы L1",
                "HIL2" => "Гармоники THD тока фазы L2",
                "HIL3" => "Гармоники THD тока фазы L3",
                "Angle1" => "Угол между фазными напряжениями 1",
                "Angle2" => "Угол между фазными напряжениями 2",
                "Angle3" => "Угол между фазными напряжениями 3",
                "AllEnergyK" => "Накопленная энергия с учетом коэффициента трансформации",
                _ => parameterName
            };
        }

        public static string GetParameterShortName(string parameterName)
        {
            return parameterName switch
            {
                "UL1N" => "U L1-N",
                "UL2N" => "U L2-N",
                "UL3N" => "U L3-N",
                "UL1L2" => "U L1-L2",
                "UL2L3" => "U L2-L3",
                "UL3L1" => "U L3-L1",
                "IL1" => "I L1",
                "IL2" => "I L2",
                "IL3" => "I L3",
                "PL1" => "P L1",
                "PL2" => "P L2",
                "PL3" => "P L3",
                "PSum" => "P Σ",
                "QL1" => "Q L1",
                "QL2" => "Q L2",
                "QL3" => "Q L3",
                "QSum" => "Q Σ",
                "AllEnergy" => "Энергия акт.",
                "ReactiveEnergySum" => "Энергия реакт.",
                "Freq" => "Частота",
                "Aq1" => "S L1",
                "Aq2" => "S L2",
                "Aq3" => "S L3",
                "AqSum" => "S Σ",
                "FundPfCf1" => "cos φ L1",
                "FundPfCf2" => "cos φ L2",
                "FundPfCf3" => "cos φ L3",
                "RotationField" => "Вращ. поле",
                "RqcL1" => "Энергия потр. L1",
                "RqcL2" => "Энергия потр. L2",
                "RqcL3" => "Энергия потр. L3",
                "RqdL1" => "Энергия отд. L1",
                "RqdL2" => "Энергия отд. L2",
                "RqdL3" => "Энергия отд. L3",
                "ReactQIL1" => "Q инд. L1",
                "ReactQIL2" => "Q инд. L2",
                "ReactQIL3" => "Q инд. L3",
                "ReactQCL1" => "Q емк. L1",
                "ReactQCL2" => "Q емк. L2",
                "ReactQCL3" => "Q емк. L3",
                "HUL1" => "THD U L1",
                "HUL2" => "THD U L2",
                "HUL3" => "THD U L3",
                "HIL1" => "THD I L1",
                "HIL2" => "THD I L2",
                "HIL3" => "THD I L3",
                "Angle1" => "Угол 1",
                "Angle2" => "Угол 2",
                "Angle3" => "Угол 3",
                "AllEnergyK" => "Энергия с коэф.",
                _ => parameterName
            };
        }

        public static string GetParameterUnit(string parameterName)
        {
            return parameterName switch
            {
                // Напряжения - В, 1 знак после запятой
                "UL1N" => "В",
                "UL2N" => "В",
                "UL3N" => "В",
                "UL1L2" => "В",
                "UL2L3" => "В",
                "UL3L1" => "В",
                
                // Токи - А, 1 знак после запятой
                "IL1" => "А",
                "IL2" => "А",
                "IL3" => "А",
                
                // Активные мощности - кВт, 1 знак после запятой
                "PL1" => "кВт",
                "PL2" => "кВт",
                "PL3" => "кВт",
                "PSum" => "кВт",
                
                // Реактивные мощности - кВар, 1 знак после запятой
                "QL1" => "кВар",
                "QL2" => "кВар",
                "QL3" => "кВар",
                "QSum" => "кВар",
                
                // Полные мощности - кВА, 1 знак после запятой
                "Aq1" => "кВА",
                "Aq2" => "кВА",
                "Aq3" => "кВА",
                "AqSum" => "кВА",
                
                // Активная энергия - кВт⋅ч, 0 знаков после запятой
                "AllEnergy" => "кВт⋅ч",
                "AllEnergyK" => "кВт⋅ч",
                "RqcL1" => "кВт⋅ч",
                "RqcL2" => "кВт⋅ч",
                "RqcL3" => "кВт⋅ч",
                "RqdL1" => "кВт⋅ч",
                "RqdL2" => "кВт⋅ч",
                "RqdL3" => "кВт⋅ч",
                
                // Реактивная энергия - кВар⋅ч, 0 знаков после запятой
                "ReactiveEnergySum" => "кВар⋅ч",
                "ReactQIL1" => "кВар⋅ч",
                "ReactQIL2" => "кВар⋅ч",
                "ReactQIL3" => "кВар⋅ч",
                "ReactQCL1" => "кВар⋅ч",
                "ReactQCL2" => "кВар⋅ч",
                "ReactQCL3" => "кВар⋅ч",
                
                // Остальные параметры
                "Freq" => "Гц",
                "FundPfCf1" => "",
                "FundPfCf2" => "",
                "FundPfCf3" => "",
                "RotationField" => "",
                "HUL1" => "%",
                "HUL2" => "%",
                "HUL3" => "%",
                "HIL1" => "%",
                "HIL2" => "%",
                "HIL3" => "%",
                "Angle1" => "°",
                "Angle2" => "°",
                "Angle3" => "°",
                _ => ""
            };
        }

        public static int GetParameterDecimalPlaces(string parameterName)
        {
            return parameterName switch
            {
                // Напряжения - 1 знак после запятой
                "UL1N" or "UL2N" or "UL3N" or "UL1L2" or "UL2L3" or "UL3L1" => 1,
                
                // Токи - 1 знак после запятой
                "IL1" or "IL2" or "IL3" => 1,
                
                // Активные мощности - 1 знак после запятой
                "PL1" or "PL2" or "PL3" or "PSum" => 1,
                
                // Реактивные мощности - 1 знак после запятой
                "QL1" or "QL2" or "QL3" or "QSum" => 1,
                
                // Полные мощности - 1 знак после запятой
                "Aq1" or "Aq2" or "Aq3" or "AqSum" => 1,
                
                // Энергии - 0 знаков после запятой
                "AllEnergy" or "AllEnergyK" or "ReactiveEnergySum" or
                "RqcL1" or "RqcL2" or "RqcL3" or "RqdL1" or "RqdL2" or "RqdL3" or
                "ReactQIL1" or "ReactQIL2" or "ReactQIL3" or "ReactQCL1" or "ReactQCL2" or "ReactQCL3" => 0,
                
                // Остальные параметры - 2 знака после запятой
                _ => 2
            };
        }

        public static decimal ConvertToDisplayValue(decimal value, string parameterName)
        {
            return parameterName switch
            {
                // Мощности - делим на 1000 для перевода в кВт/кВар/кВА
                "PL1" or "PL2" or "PL3" or "PSum" or
                "QL1" or "QL2" or "QL3" or "QSum" or
                "Aq1" or "Aq2" or "Aq3" or "AqSum" => value,
                
                // Энергии - делим на 1000 для перевода в кВт⋅ч/кВар⋅ч
                "AllEnergy" or "AllEnergyK" or "ReactiveEnergySum" or
                "RqcL1" or "RqcL2" or "RqcL3" or "RqdL1" or "RqdL2" or "RqdL3" or
                "ReactQIL1" or "ReactQIL2" or "ReactQIL3" or "ReactQCL1" or "ReactQCL2" or "ReactQCL3" => value,
                
                // Остальные параметры - без изменений
                _ => value
            };
        }
    }
} 