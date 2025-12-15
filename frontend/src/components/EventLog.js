import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5165';

function EventLog() {
  const [events, setEvents] = useState([]);
  const [objects, setObjects] = useState([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
    objectGroupId: '',
    periodType: 'new',
    startDate: '',
    endDate: ''
  });

  useEffect(() => {
    fetchObjects();
    fetchEvents();
  }, []);

  const fetchObjects = async () => {
    try {
      const response = await axios.get(`${API_URL}/api/Event/objects`);
      setObjects(response.data);
    } catch (error) {
      console.error('Error fetching objects:', error);
      // Mock data for development
      setObjects([
        { id: 1, name: 'КВТ-Юг', place: 'Тульское шоссе, 16' },
        { id: 2, name: 'КВТ-Север', place: 'Ленинский проспект, 45' },
        { id: 3, name: 'КВТ-Запад', place: 'Московское шоссе, 78' }
      ]);
    }
  };

  const fetchEvents = async () => {
    setLoading(true);
    try {
      const requestData = {
        objectGroupId: filters.objectGroupId || null,
        periodType: filters.periodType,
        startDate: filters.startDate ? new Date(filters.startDate) : null,
        endDate: filters.endDate ? new Date(filters.endDate) : null
      };

      const response = await axios.post(`${API_URL}/api/Event/log`, requestData);
      setEvents(response.data.events || []);
    } catch (error) {
      console.error('Error fetching events:', error);
      // Mock data for development
      setEvents([
        {
          id: 1,
          eventCode: '34645546546',
          eventName: 'Ошибка',
          date: '2024-04-10T12:55:00',
          objectName: 'КВТ-Юг',
          objectAddress: 'Тульское шоссе, 16',
          deviceName: 'Датчик 676-АЕМ',
          eventDescription: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt.'
        },
        {
          id: 2,
          eventCode: '34645546546',
          eventName: 'Ошибка',
          date: '2024-04-10T12:65:00',
          objectName: 'КВТ-Юг',
          objectAddress: 'Тульское шоссе, 16',
          deviceName: 'Датчик 676-АЕМ',
          eventDescription: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt.'
        },
        {
          id: 3,
          eventCode: '34645546546',
          eventName: 'Ошибка',
          date: '2024-04-10T12:55:00',
          objectName: 'КВТ-Юг',
          objectAddress: 'Тульское шоссе, 16',
          deviceName: 'Датчик 676-АЕМ',
          eventDescription: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt.'
        }
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (field, value) => {
    setFilters(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleApplyFilters = () => {
    fetchEvents();
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: '2-digit'
    }) + ' ' + date.toLocaleTimeString('ru-RU', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="w-[1515px] h-[865px] bg-white rounded-[10px] border border-neutral-400/20 mt-[30px] p-6">
      <div className="flex gap-6 h-full">
        {/* Left Panel - Filters */}
        <div className="w-80 bg-gray-50 rounded-lg p-4 flex-shrink-0">
          <h3 className="text-lg font-semibold mb-4">Период</h3>
          
          {/* Object Group Filter */}
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Группа объектов:
            </label>
            <select
              className="w-full p-2 border border-gray-300 rounded-md"
              value={filters.objectGroupId}
              onChange={(e) => handleFilterChange('objectGroupId', e.target.value)}
            >
              <option value="">Все объекты</option>
              {objects.map(obj => (
                <option key={obj.id} value={obj.id}>
                  {obj.name}
                </option>
              ))}
            </select>
          </div>

          {/* Period Type Filter */}
          <div className="mb-4">
            <div className="space-y-2">
              <label className="flex items-center">
                <input
                  type="radio"
                  name="periodType"
                  value="new"
                  checked={filters.periodType === 'new'}
                  onChange={(e) => handleFilterChange('periodType', e.target.value)}
                  className="mr-2"
                />
                <span className="text-sm">Только новые</span>
              </label>
              <label className="flex items-center">
                <input
                  type="radio"
                  name="periodType"
                  value="week"
                  checked={filters.periodType === 'week'}
                  onChange={(e) => handleFilterChange('periodType', e.target.value)}
                  className="mr-2"
                />
                <span className="text-sm">За неделю</span>
              </label>
              <label className="flex items-center">
                <input
                  type="radio"
                  name="periodType"
                  value="month"
                  checked={filters.periodType === 'month'}
                  onChange={(e) => handleFilterChange('periodType', e.target.value)}
                  className="mr-2"
                />
                <span className="text-sm">За месяц</span>
              </label>
              <label className="flex items-center">
                <input
                  type="radio"
                  name="periodType"
                  value="custom"
                  checked={filters.periodType === 'custom'}
                  onChange={(e) => handleFilterChange('periodType', e.target.value)}
                  className="mr-2"
                />
                <span className="text-sm">За период:</span>
              </label>
            </div>
          </div>

          {/* Custom Date Range */}
          {filters.periodType === 'custom' && (
            <div className="mb-4 space-y-2">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  С:
                </label>
                <input
                  type="date"
                  className="w-full p-2 border border-gray-300 rounded-md"
                  value={filters.startDate}
                  onChange={(e) => handleFilterChange('startDate', e.target.value)}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  по:
                </label>
                <input
                  type="date"
                  className="w-full p-2 border border-gray-300 rounded-md"
                  value={filters.endDate}
                  onChange={(e) => handleFilterChange('endDate', e.target.value)}
                />
              </div>
            </div>
          )}

          {/* Apply Button */}
          <button
            onClick={handleApplyFilters}
            disabled={loading}
            className="w-full bg-black text-white py-2 px-4 rounded-md hover:bg-gray-800 disabled:opacity-50"
          >
            {loading ? 'Загрузка...' : 'Применить'}
          </button>
        </div>

        {/* Right Panel - Event Log Table */}
        <div className="flex-1 overflow-hidden flex flex-col">
          <h3 className="text-lg font-semibold mb-4 flex-shrink-0">Журнал событий</h3>
          
          <div className="bg-white border border-gray-200 rounded-lg overflow-hidden flex-1">
            <div className="overflow-x-auto overflow-y-auto h-full">
              <table className="w-full min-w-[1200px]">
                <thead className="bg-gray-50 sticky top-0">
                  <tr>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-16">№</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-48">Код события</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-64">Наименование события</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-40">Дата и время</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-80">Наименование объекта</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-64">Наименование прибора</th>
                    <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 min-w-[300px]">Описание события</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {events.map((event, index) => (
                    <tr key={event.id} className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                      <td className="px-4 py-3 text-sm text-gray-900">№{index + 1}</td>
                      <td className="px-4 py-3 text-sm text-gray-900">{event.eventCode}</td>
                      <td className="px-4 py-3 text-sm text-gray-900">{event.eventName}</td>
                      <td className="px-4 py-3 text-sm text-gray-900">{formatDate(event.date)}</td>
                      <td className="px-4 py-3 text-sm text-gray-900">
                        {event.objectName}, {event.objectAddress}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-900">{event.deviceName}</td>
                      <td className="px-4 py-3 text-sm text-gray-900">
                        {event.eventDescription}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          
          {events.length === 0 && !loading && (
            <div className="text-center py-8 text-gray-500 flex-shrink-0">
              События не найдены
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default EventLog; 