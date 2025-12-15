import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

// Функция для получения локального времени в формате datetime-local
const getLocalDateTime = (date = new Date()) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  
  return `${year}-${month}-${day}T${hours}:${minutes}`;
};

function DataVisualization() {
  const [activeTab, setActiveTab] = useState('graph'); // 'graph' or 'table'
  const [period, setPeriod] = useState('last2days');
  const [dateFrom, setDateFrom] = useState(getLocalDateTime());
  const [dateTo, setDateTo] = useState(getLocalDateTime());
  const [selectedObjects, setSelectedObjects] = useState([]);
  const [selectedDeviceTypes, setSelectedDeviceTypes] = useState([]);
  const [selectedMeters, setSelectedMeters] = useState([]); // Изменено с null на [] для множественного выбора
  const [selectedParameters, setSelectedParameters] = useState([]);
  const [data, setData] = useState([]);
  const [objects, setObjects] = useState([]);
  const [deviceTypes, setDeviceTypes] = useState([]);
  const [parameters, setParameters] = useState([]);
  const [filteredMeters, setFilteredMeters] = useState([]);
  const [loading, setLoading] = useState(false);
  const [aggregation, setAggregation] = useState('hour'); // 'minute', 'hour', 'day'

  useEffect(() => {
    fetchObjects();
    fetchDeviceTypes();
  }, []);

  useEffect(() => {
    if (selectedMeters && selectedMeters.length > 0) {
      fetchParameters();
    }
  }, [selectedMeters, filteredMeters]);

  useEffect(() => {
    // Фильтруем счетчики на основе выбранных объектов и типов устройств
    const filtered = objects
      .filter(obj => selectedObjects.length === 0 || selectedObjects.includes(obj.id))
      .flatMap(obj => obj.devices)
      .filter(device => selectedDeviceTypes.length === 0 || selectedDeviceTypes.includes(device.type))
      .sort((a, b) => {
        // Сортируем по SortId, если есть, иначе по ID
        const aSortId = a.sortId ?? a.id;
        const bSortId = b.sortId ?? b.id;
        return aSortId - bSortId;
      });
    
    setFilteredMeters(filtered);
    setSelectedMeters([]); // Сбрасываем выбранный счетчик при изменении фильтров
  }, [selectedObjects, selectedDeviceTypes, objects]);

  // Убираем автоматическую загрузку - теперь данные загружаются только по кнопке "Применить"

  const fetchObjects = async () => {
    try {
      const response = await axios.get(`${API_URL}/api/Visualization/objects`);
      setObjects(response.data);
    } catch (error) {
      console.error('Error fetching objects:', error);
    }
  };

  const fetchDeviceTypes = async () => {
    try {
      const response = await axios.get(`${API_URL}/api/Visualization/device-types`);
      setDeviceTypes(response.data);
    } catch (error) {
      console.error('Error fetching device types:', error);
    }
  };

  const fetchParameters = async () => {
    try {
      // Определяем тип устройства по выбранному счетчику
      let deviceType = 'electrical'; // по умолчанию
      
      if (selectedMeters && selectedMeters.length > 0) {
        const selectedDevice = filteredMeters.find(d => d.id === selectedMeters[0]);
        if (selectedDevice && selectedDevice.type) {
          deviceType = selectedDevice.type;
        }
      }
      
      // Используем новый endpoint для получения читаемых параметров
      const response = await axios.get(`${API_URL}/api/Visualization/parameters-readable/${deviceType}`);
      setParameters(response.data);
    } catch (error) {
      console.error('Error fetching parameters:', error);
    }
  };

  const fetchData = async () => {
    setLoading(true);
    try {
      // Определяем тип устройства по выбранным счетчикам
      let meterType = 'electrical'; // по умолчанию
      
      if (selectedMeters && selectedMeters.length > 0) {
        const selectedDevice = filteredMeters.find(d => d.id === selectedMeters[0]);
        if (selectedDevice && selectedDevice.type) {
          meterType = selectedDevice.type;
        }
      }
      
      const params = new URLSearchParams();
      params.append('period', period);
      if (period === 'custom') {
        params.append('dateFrom', dateFrom);
        params.append('dateTo', dateTo);
      }
      params.append('meterType', meterType);
      params.append('aggregation', aggregation);
      
      if (selectedObjects && selectedObjects.length > 0) {
        selectedObjects.forEach(objectId => {
          params.append('objectIds', objectId);
        });
      }
      
      selectedMeters.forEach(meterId => {
        params.append('meterIds', meterId);
      });
      
      selectedParameters.forEach(param => {
        params.append('parameters', param);
      });

      const response = await axios.get(`${API_URL}/api/Visualization/data?${params}`);
      const responseData = response.data.data || [];
      
      // Отладочная информация
      console.log('=== Отладка fetchData ===');
      console.log('URL запроса:', `${API_URL}/api/Visualization/data?${params}`);
      console.log('Ответ от сервера:', response.data);
      console.log('Данные для отображения:', responseData);
      console.log('Количество записей:', responseData.length);
      if (responseData.length > 0) {
        console.log('Структура первой записи:', responseData[0]);
        console.log('Уникальные DeviceId в ответе:', [...new Set(responseData.map(item => item.deviceId))]);
        console.log('Уникальные временные метки:', [...new Set(responseData.map(item => item.timestamp))]);
      }
      console.log('========================');
      
      setData(responseData);
    } catch (error) {
      console.error('Error fetching visualization data:', error);
      setData([]);
    } finally {
      setLoading(false);
    }
  };

  const handleObjectToggle = (objectId) => {
    setSelectedObjects(prev => 
      prev.includes(objectId) 
        ? prev.filter(id => id !== objectId)
        : [...prev, objectId]
    );
  };

  const handleDeviceTypeToggle = (deviceType) => {
    setSelectedDeviceTypes(prev => 
      prev.includes(deviceType) 
        ? prev.filter(type => type !== deviceType)
        : [...prev, deviceType]
    );
  };

  const handleMeterToggle = (meterId) => {
    setSelectedMeters(prev => 
      prev.includes(meterId) 
        ? prev.filter(id => id !== meterId)
        : [...prev, meterId]
    );
  };

  const handleParameterToggle = (paramId) => {
    setSelectedParameters(prev => 
      prev.includes(paramId) 
        ? prev.filter(id => id !== paramId)
        : [...prev, paramId]
    );
  };

  const handleCreateReport = async () => {
    if (!data.length) {
      alert('Нет данных для создания отчета');
      return;
    }

    try {
      const userData = JSON.parse(localStorage.getItem("user"));
      
      // Получаем уникальные ID устройств
      const deviceIds = [...new Set(data.map(item => item.deviceId))];
      
      // Подготавливаем данные для отчета в том же формате, что и таблица
      const reportData = data.map(item => ({
        timestamp: new Date(item.timestamp),
        deviceId: item.deviceId,
        deviceName: item.deviceName,
        values: item.values
      }));

      const requestData = {
        type: "data_visualization",
        name: `Отчет по данным ${new Date().toLocaleDateString('ru-RU')}`,
        createdByUserId: userData?.id || null,
        parameters: selectedParameters,
        data: reportData
      };

      console.log('Данные для отчета:', requestData);

      const response = await axios.post(`${API_URL}/api/Report/create-visualization`, requestData);
      
      if (response.data) {
        alert('Отчет успешно создан!');
      }
    } catch (error) {
      console.error('Error creating report:', error);
      alert('Ошибка при создании отчета');
    }
  };



  const formatChartData = () => {
    if (data.length === 0) return [];
    
    // Сортируем данные по времени
    const sortedData = data.sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
    
    // Определяем временной диапазон
    const minTime = new Date(sortedData[0].timestamp);
    const maxTime = new Date(sortedData[sortedData.length - 1].timestamp);
    
    // Создаем временные интервалы в зависимости от выбранной агрегации
    const intervals = createTimeIntervals(minTime, maxTime, aggregation);
    
    // Получаем уникальные ID устройств
    const deviceIds = [...new Set(sortedData.map(item => item.deviceId))];
    
    // Агрегируем данные по интервалам и устройствам
    const result = intervals.map(interval => {
      const intervalData = sortedData.filter(item => {
        const itemTime = new Date(item.timestamp);
        return itemTime >= interval.start && itemTime < interval.end;
      });
      
      if (intervalData.length === 0) return null;
      
      // Создаем объект с данными для каждого устройства в этом интервале
      const resultItem = {
        timestamp: interval.start.toISOString(),
        displayTime: formatTimeForAxis(interval.start),
        time: interval.start
      };
      
      // Добавляем данные для каждого устройства
      deviceIds.forEach(deviceId => {
        const deviceData = intervalData.find(item => item.deviceId === deviceId);
        if (deviceData) {
          // Добавляем все значения параметров для этого устройства
          Object.entries(deviceData.values).forEach(([paramKey, value]) => {
            resultItem[`${paramKey}_${deviceId}`] = value;
            resultItem[`${paramKey}_${deviceId}_deviceName`] = deviceData.deviceName;
            resultItem[`${paramKey}_${deviceId}_objectName`] = deviceData.objectName;
          });
        }
      });
      
      return resultItem;
    }).filter(Boolean); // Убираем null значения
    
    return result;
  };

  // Создаем временные интервалы для агрегации
  const createTimeIntervals = (minTime, maxTime, aggregationType) => {
    const intervals = [];
    let currentTime = new Date(minTime);
    
    // Округляем начало до начала интервала
    switch (aggregationType) {
      case 'minute':
        currentTime.setSeconds(0, 0);
        break;
      case 'hour':
        currentTime.setMinutes(0, 0, 0);
        break;
      case 'day':
        currentTime.setHours(0, 0, 0, 0);
        break;
    }
    
    while (currentTime <= maxTime) {
      const endTime = new Date(currentTime);
      
      // Устанавливаем конец интервала
      switch (aggregationType) {
        case 'minute':
          endTime.setMinutes(endTime.getMinutes() + 1);
          break;
        case 'hour':
          endTime.setHours(endTime.getHours() + 1);
          break;
        case 'day':
          endTime.setDate(endTime.getDate() + 1);
          break;
      }
      
      intervals.push({
        start: new Date(currentTime),
        end: endTime
      });
      
      // Переходим к следующему интервалу
      switch (aggregationType) {
        case 'minute':
          currentTime.setMinutes(currentTime.getMinutes() + 1);
          break;
        case 'hour':
          currentTime.setHours(currentTime.getHours() + 1);
          break;
        case 'day':
          currentTime.setDate(currentTime.getDate() + 1);
          break;
      }
    }
    
    return intervals;
  };

  // Форматируем время для отображения на оси X
  const formatTimeForAxis = (timestamp) => {
    try {
      const date = new Date(timestamp);
      if (isNaN(date.getTime())) {
        return 'Н/Д';
      }
      
      switch (aggregation) {
        case 'minute':
          return date.toLocaleString('ru-RU', {
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
          });
        case 'hour':
          return date.toLocaleString('ru-RU', {
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit'
          });
        case 'day':
          return date.toLocaleString('ru-RU', {
            month: '2-digit',
            day: '2-digit'
          });
        default:
          return date.toLocaleString('ru-RU', {
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit'
          });
      }
    } catch (error) {
      return 'Ошибка';
    }
  };

  // Группируем параметры по типам для отображения на одном графике
  const groupParametersByType = () => {
    const groups = {};
    
    selectedParameters.forEach(paramKey => {
      // Ищем параметр в новой структуре данных
      let paramGroup = null;
      let paramItem = null;
      
      for (const group of parameters) {
        paramItem = group.parameters.find(p => p.code === paramKey);
        if (paramItem) {
          paramGroup = group;
          break;
        }
      }
      
      if (paramGroup && paramItem) {
        const groupKey = paramGroup.name.split(' ')[0]; // Берем первое слово как группу (например, "Напряжение", "Ток")
        if (!groups[groupKey]) {
          groups[groupKey] = [];
        }
        groups[groupKey].push({
          key: paramKey,
          name: paramGroup.name,
          displayName: paramItem.fullName
        });
      }
    });
    
    return groups;
  };

  // Функция для получения цвета для каждого типа параметра
  const getColorForType = (type, index) => {
    const colors = ['#ff6b35', '#4ecdc4', '#45b7d1', '#96ceb4', '#feca57', '#ff9ff3', '#54a0ff', '#5f27cd'];
    return colors[index % colors.length];
  };

  // Функция для получения уникального цвета для каждого устройства
  const getColorForDevice = (deviceId) => {
    const colors = [
      '#ff6b35', '#4ecdc4', '#45b7d1', '#96ceb4', 
      '#feca57', '#ff9ff3', '#54a0ff', '#5f27cd',
      '#ff4757', '#2ed573', '#1e90ff', '#ffa502',
      '#ff6348', '#32cd32', '#4169e1', '#daa520'
    ];
    return colors[deviceId % colors.length];
  };

  const renderGraphs = () => {
    const chartData = formatChartData();
    
    if (chartData.length === 0) {
      return <div>Нет данных для отображения</div>;
    }
    
    const parameterGroups = groupParametersByType();
    if (Object.keys(parameterGroups).length === 0) {
      return <div>Нет параметров для отображения</div>;
    }
    
    // Получаем уникальные ID устройств для создания линий
    const deviceIds = [...new Set(data.map(item => item.deviceId))];
    
    return (
      <div className="mb-6">
        {Object.entries(parameterGroups).map(([type, params]) => (
          <div key={type} className="mb-8">
            <h3 className="text-lg font-semibold mb-4">{type}</h3>
            <ResponsiveContainer width="100%" height={400}>
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis 
                  dataKey="displayTime" 
                  angle={-45}
                  textAnchor="end"
                  height={80}
                />
                <YAxis />
                <Tooltip 
                  labelFormatter={(value) => {
                    const item = chartData.find(item => item.displayTime === value);
                    return item ? new Date(item.timestamp).toLocaleString('ru-RU') : value;
                  }}
                  formatter={(value, name) => {
                    // Извлекаем имя устройства из названия линии
                    const deviceId = name.split('_').pop();
                    const deviceName = chartData[0]?.[`${name.split('_')[0]}_${deviceId}_deviceName`] || `Device ${deviceId}`;
                    return [value, deviceName];
                  }}
                />
                <Legend 
                  formatter={(value, entry) => {
                    // Упрощаем название в легенде, убирая дублирование
                    const parts = value.split(' - ');
                    if (parts.length > 1) {
                      return `${parts[0]} (${parts[1]})`;
                    }
                    return value;
                  }}
                />
                {params.map((param) => 
                  deviceIds.map((deviceId, deviceIndex) => {
                    const dataKey = `${param.key}_${deviceId}`;
                    const deviceName = chartData[0]?.[`${param.key}_${deviceId}_deviceName`] || `Device ${deviceId}`;
                    
                    return (
                      <Line
                        key={`${type}-${param.key}-${deviceId}`}
                        type="monotone"
                        dataKey={dataKey}
                        stroke={getColorForDevice(deviceId)}
                        name={`${param.displayName} - ${deviceName}`}
                        connectNulls={false}
                        strokeWidth={2}
                      />
                    );
                  })
                ).flat()}
              </LineChart>
            </ResponsiveContainer>
          </div>
        ))}
      </div>
    );
  };

  const renderTable = () => {
    if (!data.length) return <div className="text-gray-500 text-center py-4">Нет данных</div>;

    // Получаем уникальные ID устройств
    const deviceIds = [...new Set(data.map(item => item.deviceId))];
    
    // Отладочная информация
    console.log('=== Отладка таблицы ===');
    console.log('Всего записей:', data.length);
    console.log('Уникальные DeviceId:', deviceIds);
    console.log('Выбранные параметры:', selectedParameters);
    console.log('Первые несколько записей:', data.slice(0, 3));
    
    // Создаем колонки для выбранных параметров и устройств
    const columns = [
      { key: 'timestamp', name: 'Дата и время' },
      ...selectedParameters.flatMap(paramKey => 
        deviceIds.map(deviceId => {
          const deviceName = data.find(item => item.deviceId === deviceId)?.deviceName || `Device ${deviceId}`;
          // Находим читаемое название параметра
          let paramDisplayName = paramKey;
          for (const paramGroup of parameters) {
            const param = paramGroup.parameters.find(p => p.code === paramKey);
            if (param) {
              paramDisplayName = param.fullName;
              break;
            }
          }
          return {
            key: `${paramKey}_${deviceId}`,
            name: `${paramDisplayName} - ${deviceName}`,
            paramKey,
            deviceId
          };
        })
      )
    ];

    // Группируем данные по времени для правильного отображения
    const timeGroups = {};
    data.forEach(item => {
      const timeKey = item.timestamp;
      if (!timeGroups[timeKey]) {
        timeGroups[timeKey] = {};
      }
      timeGroups[timeKey][item.deviceId] = item;
    });

    // Получаем отсортированные временные метки
    const sortedTimestamps = Object.keys(timeGroups).sort();
    
    console.log('Группировка по времени:', timeGroups);
    console.log('Отсортированные временные метки:', sortedTimestamps);
    console.log('Количество временных групп:', sortedTimestamps.length);
    console.log('========================');

    return (
      <div className="bg-white rounded border overflow-auto max-h-full">
        <table className="w-full text-sm font-open-sans">
          <thead className="bg-gray-100">
            <tr>
              {columns.map((col, idx) => (
                <th key={idx} className="border p-2 text-left">{col.name}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {sortedTimestamps.map((timestamp, idx) => {
              const timeGroup = timeGroups[timestamp];
              return (
                <tr key={idx}>
                  {columns.map((col, colIdx) => (
                    <td key={colIdx} className="border p-2">
                      {col.key === 'timestamp' 
                        ? new Date(timestamp).toLocaleString('ru-RU')
                        : (() => {
                            // Находим данные для конкретного устройства и параметра
                            const deviceData = timeGroup[col.deviceId];
                            if (deviceData && deviceData.values && deviceData.values[col.paramKey] !== undefined) {
                              return deviceData.values[col.paramKey].toFixed(3);
                            }
                            return '0.000';
                          })()
                      }
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    );
  };

  return (
    <div className="w-full max-w-full h-auto min-h-[865px] mt-4 xl:mt-[30px]">
      <div className="flex flex-col xl:flex-row gap-4 xl:gap-6 h-full">
        {/* Left Control Panel */}
        <div className="w-full xl:w-72 bg-white/70 rounded-[10px] border border-neutral-400/20 p-4 flex flex-col gap-4 overflow-y-auto max-h-full">
          {/* Period Selection Section */}
          <div>
            <h3 className="font-semibold text-sm mb-3 font-open-sans text-gray-800">Период</h3>
            <div className="space-y-2 max-h-64 overflow-y-auto pr-2">
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="last2days" 
                  checked={period === 'last2days'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">За два дня</span>
              </label>
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="lastDay" 
                  checked={period === 'lastDay'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">За прошлые сутки</span>
              </label>
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="lastWeek" 
                  checked={period === 'lastWeek'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">За неделю</span>
              </label>
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="last2weeks" 
                  checked={period === 'last2weeks'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">За две недели</span>
              </label>
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="lastMonth" 
                  checked={period === 'lastMonth'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">За прошлый месяц</span>
              </label>
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="thisMonth" 
                  checked={period === 'thisMonth'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">С начала месяца</span>
              </label>
              <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                <input 
                  type="radio" 
                  name="period" 
                  value="custom" 
                  checked={period === 'custom'}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                />
                <span className="text-sm font-open-sans text-gray-700">За период:</span>
              </label>
            </div>
            
            {period === 'custom' && (
              <div className="mt-4 space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    От:
                  </label>
                  <input
                    type="datetime-local"
                    value={dateFrom}
                    onChange={(e) => setDateFrom(e.target.value)}
                    max={dateTo}
                    className="w-full p-2 border border-gray-300 rounded text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    До:
                  </label>
                  <input
                    type="datetime-local"
                    value={dateTo}
                    onChange={(e) => setDateTo(e.target.value)}
                    min={dateFrom}
                    className="w-full p-2 border border-gray-300 rounded text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500"
                  />
                </div>
                {dateFrom && dateTo && dateFrom > dateTo && (
                  <div className="text-red-500 text-xs">
                    Дата "До" не может быть раньше даты "От"
                  </div>
                )}
                <button
                  onClick={() => {
                    const twoDaysAgo = new Date();
                    twoDaysAgo.setDate(twoDaysAgo.getDate() - 2);
                    setDateFrom(getLocalDateTime(twoDaysAgo));
                    setDateTo(getLocalDateTime());
                  }}
                  className="w-full px-3 py-2 text-xs bg-gray-100 text-gray-600 rounded hover:bg-gray-200 transition-colors"
                >
                  Последние 2 дня
                </button>
              </div>
            )}
            
            {/* Aggregation Selection */}
            <div className="mt-4">
              <h4 className="font-semibold text-sm mb-3 font-open-sans text-gray-800">Агрегация данных</h4>
              <div className="space-y-2">
                <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                  <input 
                    type="radio" 
                    name="aggregation" 
                    value="minute" 
                    checked={aggregation === 'minute'}
                    onChange={(e) => setAggregation(e.target.value)}
                    className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                  />
                  <span className="text-sm font-open-sans text-gray-700">По минутам</span>
                </label>
                <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                  <input 
                    type="radio" 
                    name="aggregation" 
                    value="hour" 
                    checked={aggregation === 'hour'}
                    onChange={(e) => setAggregation(e.target.value)}
                    className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                  />
                  <span className="text-sm font-open-sans text-gray-700">По часам</span>
                </label>
                <label className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer transition-colors duration-200">
                  <input 
                    type="radio" 
                    name="aggregation" 
                    value="day" 
                    checked={aggregation === 'day'}
                    onChange={(e) => setAggregation(e.target.value)}
                    className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                  />
                  <span className="text-sm font-open-sans text-gray-700">По дням</span>
                </label>
              </div>
            </div>
          </div>

          {/* Objects Section */}
          <div className="mt-4">
            <h3 className="font-semibold text-sm mb-3 font-open-sans text-gray-800">Список объектов</h3>
            <div className="space-y-2 max-h-48 overflow-y-auto pr-2">
              {objects.map((obj) => (
                <label key={obj.id} className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedObjects.includes(obj.id)}
                    onChange={() => handleObjectToggle(obj.id)}
                    className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                  />
                  <span className="text-sm font-open-sans text-gray-700">{obj.name}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Device Types Section */}
          <div className="mt-4">
            <h3 className="font-semibold text-sm mb-3 font-open-sans text-gray-800">Тип устройства</h3>
            <div className="space-y-2 max-h-32 overflow-y-auto pr-2">
              {deviceTypes.map((deviceType) => (
                <label key={deviceType.id} className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedDeviceTypes.includes(deviceType.type)}
                    onChange={() => handleDeviceTypeToggle(deviceType.type)}
                    className="mr-2 w-4 h-4 text-red-700 border-gray-300 focus:ring-red-500"
                  />
                  <span className="text-sm font-open-sans text-gray-700">
                    {deviceType.type === 'electrical' ? 'Электрические' : 
                     deviceType.type === 'gas' ? 'Газовые' : 
                     deviceType.type === 'water' ? 'Водяные' : deviceType.type}
                  </span>
                </label>
              ))}
            </div>
          </div>

          {/* Meters Section */}
          <div className="mt-4">
            <h3 className="font-semibold text-sm mb-3 font-open-sans text-gray-800">Счетчики</h3>
            <div className="space-y-2 max-h-64 overflow-y-auto pr-2">
              {filteredMeters.map((meter) => (
                <div 
                  key={meter.id} 
                  className={`p-2 rounded cursor-pointer transition-colors ${
                    selectedMeters.includes(meter.id)
                      ? 'bg-red-100 border border-red-300' 
                      : 'hover:bg-gray-50'
                  }`}
                  onClick={() => handleMeterToggle(meter.id)}
                >
                  <div className="flex flex-col">
                    <span className="text-sm font-open-sans text-gray-700 font-medium">{meter.name}</span>
                    <span className="text-xs text-gray-500">
                      {meter.type === 'electrical' ? '⚡ Электрический' : 
                       meter.type === 'gas' ? '🔥 Газовый' : 
                       meter.type === 'water' ? '💧 Водяной' : meter.type}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>


          {/* Apply Button */}
          <button
            onClick={fetchData}
            disabled={loading}
            className="bg-black text-white px-4 py-2 rounded-full text-sm font-semibold hover:bg-gray-800 transition-colors mt-auto disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Загрузка...' : 'Применить'}
          </button>
        </div>

        {/* Central Data Display */}
        <div className="w-full xl:w-[865px] h-auto min-h-[865px] bg-white/70 rounded-[10px] border border-neutral-400/20 p-4 flex flex-col">
          {/* Tabs */}
          <div className="flex flex-col lg:flex-row justify-between items-center mb-4 gap-4">
            <div className="flex">
              <button 
                className={`px-6 py-3 rounded-t-lg font-open-sans text-sm font-medium transition-colors ${
                  activeTab === 'graph' 
                    ? 'bg-red-700 text-white shadow-sm' 
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
                onClick={() => setActiveTab('graph')}
              >
                График данных
              </button>
              <button 
                className={`px-6 py-3 rounded-t-lg font-open-sans text-sm font-medium transition-colors ${
                  activeTab === 'table' 
                    ? 'bg-red-700 text-white shadow-sm' 
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
                onClick={() => setActiveTab('table')}
              >
                Таблица данных
              </button>
            </div>
            
            {activeTab === 'table' && data.length > 0 && (
              <button 
                className="px-4 py-2 bg-green-600 text-white rounded-md font-open-sans text-sm font-medium hover:bg-green-700 transition-colors"
                onClick={handleCreateReport}
              >
                Создать отчет
              </button>
            )}
          </div>

          {/* Content Area */}
          <div className="flex-1 bg-gray-50 rounded-lg p-4 overflow-auto">
            {activeTab === 'graph' ? (
              <div className="space-y-4">
                {loading ? (
                  <div className="text-center py-8">
                    <div className="text-gray-500 font-open-sans">Загрузка графиков...</div>
                  </div>
                ) : selectedParameters.length > 0 ? (
                  renderGraphs()
                ) : (
                  <div className="text-center py-8">
                    <div className="text-gray-500 font-open-sans">Выберите параметры для отображения</div>
                  </div>
                )}
              </div>
            ) : (
              <div>
                {loading ? (
                  <div className="text-center py-8">
                    <div className="text-gray-500 font-open-sans">Загрузка таблицы...</div>
                  </div>
                ) : (
                  renderTable()
                )}
              </div>
            )}
          </div>
        </div>

        {/* Right Parameters Panel */}
        <div className="w-full xl:w-[362px] bg-white/70 rounded-[10px] border border-neutral-400/20 p-4 flex flex-col">
          <h3 className="font-semibold text-sm mb-3 font-open-sans text-gray-800">Параметры</h3>
          
          <div className="space-y-2">
            {parameters.map((paramGroup) => (
              <div key={paramGroup.key} className="border-b border-gray-200 pb-3 mb-3">
                <label className="flex items-center justify-between p-2 hover:bg-gray-50 rounded cursor-pointer">
                  <span className="text-sm font-open-sans font-medium text-gray-700">{paramGroup.name}</span>
                  <svg className="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </label>
                <div className="ml-4 mt-2 space-y-1 max-h-32 overflow-y-auto">
                  {paramGroup.parameters.map((param) => (
                    <label key={param.code} className="flex items-center hover:bg-gray-50 p-1 rounded cursor-pointer">
                      <input 
                        type="checkbox" 
                        checked={selectedParameters.includes(param.code)}
                        onChange={() => handleParameterToggle(param.code)}
                        className="mr-2 w-4 h-4 text-red-700 border-gray-300 rounded focus:ring-red-500"
                      />
                      <span className="text-sm font-open-sans text-gray-600" title={param.shortName}>
                        {param.fullName}
                      </span>
                    </label>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

export default DataVisualization; 