import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import axios from 'axios';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { getApiUrl, getSignalRUrl } from '../config';

// Функция для преобразования названий параметров в читаемый вид
const getParameterDisplayName = (paramName) => {
  const displayNames = {
    'UL1N': 'U L1-N',
    'UL2N': 'U L2-N', 
    'UL3N': 'U L3-N',
    'UL1L2': 'U L1-L2',
    'UL2L3': 'U L2-L3',
    'UL3L1': 'U L3-L1',
    'IL1': 'I L1',
    'IL2': 'I L2',
    'IL3': 'I L3',
    'PL1': 'P L1',
    'PL2': 'P L2',
    'PL3': 'P L3',
    'PSum': 'P Sum',
    'QL1': 'Q L1',
    'QL2': 'Q L2',
    'QL3': 'Q L3',
    'QSum': 'Q Sum',
    'AllEnergy': 'All Energy',
    'ReactiveEnergySum': 'Reactive Energy Sum',
    'Freq': 'Frequency',
    'Aq1': 'S L1',
    'Aq2': 'S L2',
    'Aq3': 'S L3',
    'AqSum': 'S Σ',
    'FundPfCf1': 'Fund PF Cf L1',
    'FundPfCf2': 'Fund PF Cf L2',
    'FundPfCf3': 'Fund PF Cf L3',
    'RotationField': 'Rotation Field',
    'RqcL1': 'Rqc L1',
    'RqcL2': 'Rqc L2',
    'RqcL3': 'Rqc L3',
    'RqdL1': 'Rqd L1',
    'RqdL2': 'Rqd L2',
    'RqdL3': 'Rqd L3',
    'ReactQIL1': 'React QI L1',
    'ReactQIL2': 'React QI L2',
    'ReactQIL3': 'React QI L3',
    'ReactQCL1': 'React QC L1',
    'ReactQCL2': 'React QC L2',
    'ReactQCL3': 'React QC L3',
    'HUL1': 'H U L1',
    'HUL2': 'H U L2',
    'HUL3': 'H U L3',
    'HIL1': 'H I L1',
    'HIL2': 'H I L2',
    'HIL3': 'H I L3',
    'Angle1': 'Angle 1',
    'Angle2': 'Angle 2',
    'Angle3': 'Angle 3',
    'AllEnergyK': 'All Energy K'
  };
  
  return displayNames[paramName] || paramName;
};

// Функция для получения единиц измерения параметров
const getParameterUnit = (paramName) => {
  const units = {
    'UL1N': 'V',
    'UL2N': 'V',
    'UL3N': 'V',
    'UL1L2': 'V',
    'UL2L3': 'V',
    'UL3L1': 'V',
    'IL1': 'A',
    'IL2': 'A',
    'IL3': 'A',
    'PL1': 'W',
    'PL2': 'W',
    'PL3': 'W',
    'PSum': 'W',
    'QL1': 'var',
    'QL2': 'var',
    'QL3': 'var',
    'QSum': 'var',
    'AllEnergy': 'kWh',
    'ReactiveEnergySum': 'kvarh',
    'Freq': 'Hz',
    'Aq1': 'кВА',
    'Aq2': 'кВА',
    'Aq3': 'кВА',
    'AqSum': 'кВА',
    'FundPfCf1': '',
    'FundPfCf2': '',
    'FundPfCf3': '',
    'RotationField': '',
    'RqcL1': 'kWh',
    'RqcL2': 'kWh',
    'RqcL3': 'kWh',
    'RqdL1': 'kWh',
    'RqdL2': 'kWh',
    'RqdL3': 'kWh',
    'ReactQIL1': 'kvarh',
    'ReactQIL2': 'kvarh',
    'ReactQIL3': 'kvarh',
    'ReactQCL1': 'kvarh',
    'ReactQCL2': 'kvarh',
    'ReactQCL3': 'kvarh',
    'HUL1': '%',
    'HUL2': '%',
    'HUL3': '%',
    'HIL1': '%',
    'HIL2': '%',
    'HIL3': '%',
    'Angle1': '°',
    'Angle2': '°',
    'Angle3': '°',
    'AllEnergyK': 'kWh'
  };
  
  return units[paramName] || '';
};

// Функция для фильтрации параметров для отображения на карточках счетчиков
const filterParametersForCards = (parameters) => {
  if (!parameters || parameters.length === 0) return [];
  
  console.log('🔍 Filtering parameters for cards:', parameters);
  
  // Определяем приоритетные параметры для карточек
  const priorityParams = [
    'IL1', 'IL2', 'IL3',        // Токи по трем фазам
    'PSum',                     // Суммарная активная мощность
    'QSum',                     // Суммарная реактивная мощность
    'AllEnergy'                 // Накопленная энергия
  ];
  
  // Сначала ищем приоритетные параметры
  const filteredParams = [];
  
  for (const priorityParam of priorityParams) {
    const foundParam = parameters.find(param => 
      param.parameterCode === priorityParam || 
      param.shortName === priorityParam || 
      param.name === priorityParam
    );
    if (foundParam) {
      filteredParams.push(foundParam);
      console.log(`✅ Found priority parameter: ${priorityParam}`);
    } else {
      console.log(`❌ Priority parameter not found: ${priorityParam}`);
    }
  }
  
  // Если приоритетных параметров меньше 6, добавляем остальные до 6 штук
  if (filteredParams.length < 6) {
    const remainingParams = parameters.filter(param => 
      !filteredParams.some(fp => 
        fp.parameterCode === param.parameterCode || 
        fp.shortName === param.shortName || 
        fp.name === param.name
      )
    );
    
    const additionalCount = Math.min(6 - filteredParams.length, remainingParams.length);
    filteredParams.push(...remainingParams.slice(0, additionalCount));
    
    console.log(`➕ Added ${additionalCount} additional parameters`);
  }
  
  const result = filteredParams.slice(0, 6); // Максимум 6 параметров на карточке
  console.log('🎯 Final filtered parameters for cards:', result);
  
  return result;
};

function DeviceAccounting() {
  const [devices, setDevices] = useState([]);
  const [objects, setObjects] = useState([]);
  const [tab, setTab] = useState(0);
  const [selectedDevice, setSelectedDevice] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [loadingDevice, setLoadingDevice] = useState(false);
  const [connection, setConnection] = useState(null);
  
  // Используем useRef для хранения актуального состояния
  const selectedDeviceRef = useRef(null);
  const showModalRef = useRef(false);

  useEffect(() => {
    fetchDevices();
  }, []);

  // Синхронизируем useRef с состоянием
  useEffect(() => {
    selectedDeviceRef.current = selectedDevice;
  }, [selectedDevice]);

  useEffect(() => {
    showModalRef.current = showModal;
  }, [showModal]);

  // SignalR подключение
  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(getSignalRUrl())
      .configureLogging(LogLevel.Information)
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          connection.invoke('JoinNotificationGroup');
        })
        .catch(err => console.error('❌ SignalR Connection Error: ', err));

      // Обработчик обновлений данных устройств
      connection.on('DeviceDataUpdate', (deviceData) => {
        updateDeviceData(deviceData);
      });

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  const updateDeviceData = useCallback((deviceData) => {
    console.log('🔄 SignalR update received:', deviceData);
    
    setDevices(prevDevices => {
      const updatedDevices = prevDevices.map(device => {
        const updatedDevice = deviceData.find(d => d.deviceId === device.id);
        if (updatedDevice) {
          console.log(`📱 Updating device ${device.id}:`, updatedDevice);
          
          // Если у нас есть новая структура с parameters, используем её
          if (updatedDevice.parameters && updatedDevice.parameters.length > 0) {
            const allParams = updatedDevice.parameters.map(param => ({
              name: param.parameterName || param.name,
              shortName: param.parameterShortName || param.parameterCode || param.shortName,
              fullName: param.parameterName || param.name,
              value: param.value?.toString() || '0',
              unit: param.unit || '',
              hasValue: param.hasValue !== false, // По умолчанию считаем, что значение есть
              parameterCode: param.parameterCode || param.shortName || param.name
            }));
            
            console.log(`🔧 Device ${device.id} parameters after update:`, allParams);
            // Проверяем наличие параметров полной мощности
            const apparentPowerParams = allParams.filter(p => 
              p.parameterCode === 'Aq1' || 
              p.parameterCode === 'Aq2' || 
              p.parameterCode === 'Aq3' || 
              p.parameterCode === 'AqSum'
            );
            if (apparentPowerParams.length > 0) {
              console.log(`⚡ Found apparent power parameters for device ${device.id}:`, apparentPowerParams);
            }
            
            return {
              ...device,
              statusColor: updatedDevice.statusColor,
              sortId: updatedDevice.sortId, // Обновляем SortId
              params: allParams // Сохраняем все параметры для модального окна
            };
          }
          // Fallback на старую логику для совместимости
          else if (updatedDevice.averageValues) {
            const formattedParams = Object.entries(updatedDevice.averageValues).map(([key, value]) => ({
              name: getParameterDisplayName(key),
              shortName: key,
              fullName: getParameterDisplayName(key),
              value: value.toString(),
              unit: getParameterUnit(key),
              parameterCode: key
            }));

            console.log(`🔧 Device ${device.id} parameters from averageValues:`, formattedParams);

            return {
              ...device,
              statusColor: updatedDevice.statusColor,
              sortId: updatedDevice.sortId, // Обновляем SortId
              params: formattedParams
            };
          }
        }
        return device;
      });
      
      // Сортируем обновленные устройства по SortId
      return updatedDevices.sort((a, b) => {
        const aSortId = a.sortId ?? a.id;
        const bSortId = b.sortId ?? b.id;
        return aSortId - bSortId;
      });
    });

    // Также обновляем данные в модальном окне, если оно открыто
    if (selectedDeviceRef.current && showModalRef.current) {
      const updatedDevice = deviceData.find(d => d.deviceId === selectedDeviceRef.current.deviceId);
      
      if (updatedDevice) {
        let formattedParameters = [];
        
        // Если у нас есть новая структура с parameters, используем её
        if (updatedDevice.parameters && updatedDevice.parameters.length > 0) {
          formattedParameters = updatedDevice.parameters.map(param => ({
            name: param.parameterName || param.name,
            shortName: param.parameterShortName || param.parameterCode || param.shortName,
            fullName: param.parameterName || param.name,
            value: param.value?.toString() || '0',
            unit: param.unit || '',
            hasValue: param.hasValue !== false // По умолчанию считаем, что значение есть
          }));
        }
        // Fallback на старую логику для совместимости
        else if (updatedDevice.averageValues) {
          formattedParameters = Object.entries(updatedDevice.averageValues).map(([key, value]) => ({
            name: getParameterDisplayName(key),
            shortName: key,
            fullName: getParameterDisplayName(key),
            value: value.toString(),
            unit: getParameterUnit(key)
          }));
        }

        setSelectedDevice(prev => ({
          ...prev,
          isActive: updatedDevice.statusColor === 'green',
          lastReading: updatedDevice.lastUpdate,
          parameters: formattedParameters
        }));
      }
    }
  }, []);

  const fetchDevices = async () => {
    try {
      const response = await axios.get(getApiUrl('/api/Device/dashboard'));
      // Сохраняем объекты и сортируем по ID для стабильности
      const sortedObjects = (response.data.objects || []).sort((a, b) => a.id - b.id);
      setObjects(sortedObjects);
      // Извлекаем устройства из структуры ответа и сортируем по SortId, затем по ID
      const allDevices = response.data.objects?.flatMap(obj => 
        obj.devices?.map(dev => ({ ...dev, object: obj.name })) || []
      ).sort((a, b) => {
        // Сортируем по SortId, если есть, иначе по ID
        const aSortId = a.sortId ?? a.id;
        const bSortId = b.sortId ?? b.id;
        return aSortId - bSortId;
      }) || [];
      
      // Добавляем логирование для отладки
      console.log('📊 Devices loaded from API:', allDevices);
      
      setDevices(allDevices);
    } catch (error) {
      console.error('Error fetching devices:', error);
      // Mock data for development
      const mockObjects = [
        { id: 1, name: 'Объект 1' },
        { id: 2, name: 'Объект 2' },
        { id: 3, name: 'Объект 3' }
      ];
      setObjects(mockObjects);
      setDevices([
        {
          id: 1,
          name: 'Прибор 1',
          object: 'Объект 1',
          params: [
            { name: 'I L1', shortName: 'IL1', fullName: 'Ток фазы L1', value: '2.5', unit: 'A', parameterCode: 'IL1' },
            { name: 'I L2', shortName: 'IL2', fullName: 'Ток фазы L2', value: '2.3', unit: 'A', parameterCode: 'IL2' },
            { name: 'I L3', shortName: 'IL3', fullName: 'Ток фазы L3', value: '2.7', unit: 'A', parameterCode: 'IL3' },
            { name: 'P Sum', shortName: 'PSum', fullName: 'Суммарная активная мощность', value: '1500', unit: 'W', parameterCode: 'PSum' },
            { name: 'Q Sum', shortName: 'QSum', fullName: 'Суммарная реактивная мощность', value: '300', unit: 'var', parameterCode: 'QSum' },
            { name: 'All Energy', shortName: 'AllEnergy', fullName: 'Накопленная энергия', value: '1250.5', unit: 'kWh', parameterCode: 'AllEnergy' }
          ]
        },
        {
          id: 2,
          name: 'Прибор 2',
          object: 'Объект 2',
          params: []
        },
        {
          id: 3,
          name: 'Прибор 3',
          object: 'Объект 1',
          params: []
        }
      ]);
    }
  };

  // Мемоизируем отфильтрованные устройства для стабильности
  const filteredDevices = useMemo(() => {
    if (tab === 0) return devices;
    return devices.filter(dev => dev.object === objects[tab - 1]?.name);
  }, [devices, objects, tab]);

  const handleDeviceClick = async (deviceId) => {
    setLoadingDevice(true);
    try {
      const response = await axios.get(getApiUrl(`/api/Device/device/${deviceId}/details`));
      
      // Устанавливаем состояние
      setSelectedDevice(response.data);
      setShowModal(true);
      
    } catch (error) {
      console.error('❌ Error fetching device details:', error);
      // Показываем базовую информацию об устройстве, если детали не загрузились
      const device = devices.find(d => d.id === deviceId);
      if (device) {
        const fallbackDevice = {
          deviceId: device.id,
          deviceName: device.deviceName || device.name,
          objectName: device.object,
          isActive: device.statusColor === 'green',
          lastReading: null,
          parameters: device.params || [],
          sortId: device.sortId
        };
        setSelectedDevice(fallbackDevice);
        setShowModal(true);
      }
    } finally {
      setLoadingDevice(false);
    }
  };

  return (
    <div className="w-full max-w-full h-auto min-h-[865px] bg-white rounded-[10px] border border-neutral-400/20 mt-4 xl:mt-[30px] p-4 lg:p-6">
      {/* Filter Tabs */}
      <div className="flex flex-wrap gap-2 lg:gap-4 mb-6">
        <button 
          className={`w-full sm:w-32 h-8 px-4 py-1 rounded-[100px] inline-flex justify-start items-center gap-2.5 ${tab === 0 ? 'bg-red-700' : 'bg-white border border-gray-300'}`} 
          onClick={() => setTab(0)}
        >
          <div className={`text-center justify-start text-sm font-semibold font-open-sans ${tab === 0 ? 'text-white' : 'text-black'}`}>
            Все приборы
          </div>
        </button>
        {objects.map((obj, idx) => (
          <button 
            key={obj.id} 
            className={`w-full sm:w-32 h-8 px-4 py-1 rounded-[100px] inline-flex justify-start items-center gap-2.5 ${tab === idx + 1 ? 'bg-red-700' : 'bg-white border border-gray-300'}`}
            onClick={() => setTab(idx + 1)}
          >
            <div className={`text-center justify-start text-sm font-semibold font-open-sans ${tab === idx + 1 ? 'text-white' : 'text-black'}`}>
              {obj.name}
            </div>
          </button>
        ))}
      </div>

      {/* Divider Line */}
      <div className="w-full h-0 outline outline-1 outline-offset-[-0.50px] outline-black/10 mb-8"></div>

      {/* Device Cards Grid */}
      <div className="grid-responsive overflow-y-auto max-h-[700px]">
        {filteredDevices && filteredDevices.length > 0 ? (
          filteredDevices.map((dev) => (
            <div 
              key={`device-${dev.id}`} 
              className="w-full max-w-64 h-80 bg-white rounded-[10px] border border-neutral-400/50 cursor-pointer hover:shadow-md p-4 flex flex-col"
              onClick={() => handleDeviceClick(dev.id)}
            >
              {/* Card Header with adaptive white pill */}
              <div className={`w-full h-8 ${dev.statusColor === 'green' ? 'bg-green-700' : 'bg-red-700'} rounded-[10px] flex items-center px-2 mb-3`}>
                <div className="h-6 bg-white rounded-lg flex items-center justify-center px-2 min-w-fit">
                  <div className="opacity-70 justify-start text-black text-sm font-semibold font-open-sans whitespace-nowrap">
                    {dev.object || `Объект ${Math.floor(dev.id / 3) + 1}`}
                  </div>
                </div>
              </div>
              
              {/* Device Title */}
              <h3 className="font-bold mb-1 text-sm text-black font-open-sans truncate">{dev.deviceName || dev.name || `Прибор ${dev.id}`}</h3>
              
              {/* Divider Line */}
              <div className="w-full h-0 outline outline-1 outline-offset-[-0.50px] outline-black/10 mb-3"></div>
              
              {/* Parameters */}
              <div className="space-y-1 flex-1">
                {dev.params && dev.params.length > 0 ? (
                  filterParametersForCards(dev.params).map((param, paramIdx) => (
                    <div key={`param-${dev.id}-${paramIdx}`} className="flex justify-between items-center">
                      <div className="opacity-50 justify-start text-black text-xs font-normal font-open-sans">
                        {param.shortName || param.name}:
                      </div>
                      <span className={`text-black font-medium text-xs font-open-sans ${param.hasValue === false ? 'text-gray-400' : ''}`}>
                        {param.hasValue === false ? 'Н/Д' : param.value}
                      </span>
                    </div>
                  ))
                ) : (
                  <div className="text-gray-500 text-center py-4 font-open-sans text-sm">
                    Данных нет
                  </div>
                )}
                {dev.params && dev.params.length > 6 && (
                  <div className="text-gray-400 text-center text-xs font-open-sans">
                    +{dev.params.length - 6} параметров
                  </div>
                )}
              </div>
            </div>
          ))
        ) : (
          <div className="col-span-5 flex items-center justify-center text-gray-500 font-open-sans">
            Загрузка устройств...
          </div>
        )}
      </div>

      {/* Device Details Modal */}
      {showModal && selectedDevice && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-4xl w-full mx-4 max-h-[80vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold">{selectedDevice.deviceName || 'Устройство'}</h2>
              <button 
                onClick={() => setShowModal(false)}
                className="text-gray-500 hover:text-gray-700"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            {loadingDevice ? (
              <div className="flex items-center justify-center py-12">
                <div className="text-center">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-700 mx-auto mb-4"></div>
                  <p className="text-gray-600">Загрузка данных устройства...</p>
                </div>
              </div>
            ) : (
              <div className="space-y-6">
                <div className="grid grid-cols-2 gap-6">
                  <div>
                    <h3 className="font-semibold mb-3 text-lg">Основная информация</h3>
                    <div className="space-y-3 text-sm">
                      <div className="flex justify-between">
                        <span className="text-gray-600">ID:</span>
                        <span className="font-medium">{selectedDevice.deviceId || 'Не указан'}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Название:</span>
                        <span className="font-medium">{selectedDevice.deviceName || 'Не указано'}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Объект:</span>
                        <span className="font-medium">{selectedDevice.objectName || 'Не указан'}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">ID сортировки:</span>
                        <span className="font-medium">{selectedDevice.sortId || 'Не указан'}</span>
                      </div>
                    </div>
                  </div>
                  
                  <div>
                    <h3 className="font-semibold mb-3 text-lg">Статус</h3>
                    <div className="space-y-3 text-sm">
                      <div className="flex justify-between">
                        <span className="text-gray-600">Статус:</span>
                        <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                          selectedDevice.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }`}>
                          {selectedDevice.isActive ? 'Онлайн' : 'Офлайн'}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Последнее обновление:</span>
                        <span className="font-medium">
                          {selectedDevice.lastReading 
                            ? new Date(selectedDevice.lastReading).toLocaleString('ru-RU')
                            : 'Неизвестно'
                          }
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div>
                  <h3 className="font-semibold mb-3 text-lg">Параметры устройства</h3>
                  {selectedDevice.parameters && selectedDevice.parameters.length > 0 ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                      {selectedDevice.parameters.map((param, idx) => (
                        <div key={`modal-param-${param.shortName || idx}`} className={`flex justify-between items-center p-3 rounded-lg border ${
                          param.hasValue === false ? 'bg-gray-100 opacity-60' : 'bg-gray-50'
                        }`}>
                          <div className="flex flex-col">
                            <span className="font-medium text-sm">{param.fullName || param.name || param.shortName}</span>
                            <span className="text-xs text-gray-500">{param.shortName || param.name}</span>
                          </div>
                          <div className="text-right">
                            <span className={`font-semibold text-lg ${param.hasValue === false ? 'text-gray-400' : 'text-black'}`}>
                              {param.hasValue === false ? 'Н/Д' : param.value}
                            </span>
                            {param.unit && param.hasValue !== false && <span className="text-xs text-gray-500 ml-1">{param.unit}</span>}
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="text-gray-500 text-center py-8 bg-gray-50 rounded-lg">
                      <svg className="w-12 h-12 mx-auto mb-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                      <p className="text-lg font-medium">Параметры не найдены</p>
                      <p className="text-sm">Данные для этого устройства отсутствуют</p>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default DeviceAccounting;