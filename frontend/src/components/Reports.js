import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5165';

const Reports = () => {
    const [reports, setReports] = useState([]);
    const [loading, setLoading] = useState(false);
    const [sortBy, setSortBy] = useState('date');
    const [sortOrder, setSortOrder] = useState('desc');
    // Получаем текущий месяц
    const getCurrentMonthDates = () => {
        const now = new Date();
        const year = now.getFullYear();
        const month = now.getMonth();
        
        // Первый день текущего месяца
        const firstDay = new Date(year, month, 1);
        // Последний день текущего месяца
        const lastDay = new Date(year, month + 1, 0);
        
        return {
            start: firstDay.toISOString().split('T')[0],
            end: lastDay.toISOString().split('T')[0]
        };
    };

    const currentMonth = getCurrentMonthDates();
    const [startDate, setStartDate] = useState(currentMonth.start);
    const [endDate, setEndDate] = useState(currentMonth.end);

    useEffect(() => {
        fetchReports();
    }, [sortBy, sortOrder, startDate, endDate]);

    const fetchReports = async () => {
        setLoading(true);
        try {
            console.log('Отправляем параметры:', { sortBy, sortOrder, startDate, endDate });
            const response = await axios.get(`${API_URL}/api/Report/list`, {
                params: {
                    sortBy,
                    sortOrder,
                    periodType: 'custom',
                    startDate,
                    endDate
                }
            });
            setReports(response.data.reports || []);
        } catch (error) {
            console.error('Error fetching reports:', error);
            // Mock data for development
            setReports([
                {
                    id: 1,
                    name: '№1',
                    createdAt: '2024-04-10T12:65:00',
                    createdByUserName: 'Прохорцев',
                    createdByUserSurname: 'Сергей'
                },
                {
                    id: 2,
                    name: '№2',
                    createdAt: '2024-04-10T12:65:00',
                    createdByUserName: 'Прохорцев',
                    createdByUserSurname: 'Сергей'
                },
                {
                    id: 3,
                    name: '№3',
                    createdAt: '2024-04-10T12:65:00',
                    createdByUserName: 'Прохорцев',
                    createdByUserSurname: 'Сергей'
                }
            ]);
        } finally {
            setLoading(false);
        }
    };

    const handleSort = (field) => {
        if (sortBy === field) {
            setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
        } else {
            setSortBy(field);
            setSortOrder('desc');
        }
    };

    const handleApplyFilters = () => {
        fetchReports();
    };

    const resetToCurrentMonth = () => {
        const currentMonth = getCurrentMonthDates();
        setStartDate(currentMonth.start);
        setEndDate(currentMonth.end);
    };

    const handleDownload = async (reportId) => {
        try {
            const response = await axios.get(`${API_URL}/api/Report/download/${reportId}`, {
                responseType: 'blob'
            });
            
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `report_${reportId}.xlsx`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Error downloading report:', error);
            alert('Ошибка при скачивании отчета');
        }
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        // Принудительно используем локальную временную зону
        return date.toLocaleDateString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: '2-digit',
            timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone
        });
    };

    const formatTime = (dateString) => {
        const date = new Date(dateString);
        // Принудительно используем локальную временную зону
        return date.toLocaleTimeString('ru-RU', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone
        });
    };

    return (
        <div className="w-[1515px] h-[865px] bg-white rounded-[10px] border border-neutral-400/20 mt-[30px] p-6">
            <div className="flex gap-6 h-full">
                {/* Left Panel - Filters */}
                <div className="w-80 bg-gray-50 rounded-lg p-4 flex-shrink-0">
                    <h3 className="text-lg font-semibold mb-4">Фильтры</h3>
                    

                    {/* Custom Date Range */}
                    <div className="mb-4 space-y-2">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                с:
                            </label>
                            <input
                                type="date"
                                className="w-full p-2 border border-gray-300 rounded-md"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                по:
                            </label>
                            <input
                                type="date"
                                className="w-full p-2 border border-gray-300 rounded-md"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                            />
                        </div>
                    </div>

                    {/* Apply Button */}
                    <button 
                        onClick={handleApplyFilters}
                        className="w-full bg-black text-white py-2 px-4 rounded-md hover:bg-gray-800 disabled:opacity-50 mb-2"
                    >
                        Применить
                    </button>

                    {/* Reset to Current Month Button */}
                    <button 
                        onClick={resetToCurrentMonth}
                        className="w-full bg-gray-500 text-white py-2 px-4 rounded-md hover:bg-gray-600 disabled:opacity-50"
                    >
                        Текущий месяц
                    </button>
                </div>

                {/* Right Panel - Reports Table */}
                <div className="flex-1 overflow-hidden flex flex-col">
                    <h3 className="text-lg font-semibold mb-4 flex-shrink-0">Отчеты</h3>
                    
                    <div className="bg-white border border-gray-200 rounded-lg overflow-hidden flex-1">
                        <div className="overflow-x-auto overflow-y-auto h-full">
                            <table className="w-full min-w-[800px]">
                                <thead className="bg-gray-50 sticky top-0">
                                    <tr>
                                        <th 
                                            className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-64 cursor-pointer hover:bg-gray-100"
                                            onClick={() => handleSort('name')}
                                        >
                                            <div className="flex items-center gap-2">
                                                Название файла
                                                {sortBy === 'name' && (
                                                    <span className="text-xs">
                                                        {sortOrder === 'asc' ? '↑' : '↓'}
                                                    </span>
                                                )}
                                            </div>
                                        </th>
                                        <th 
                                            className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-32 cursor-pointer hover:bg-gray-100"
                                            onClick={() => handleSort('date')}
                                        >
                                            <div className="flex items-center gap-2">
                                                Дата
                                                {sortBy === 'date' && (
                                                    <span className="text-xs">
                                                        {sortOrder === 'asc' ? '↑' : '↓'}
                                                    </span>
                                                )}
                                            </div>
                                        </th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-32">Время</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-64">Кто создал данный отчет</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-200">
                                    {reports.map((report, index) => (
                                        <tr key={report.id} className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                            <td className="px-4 py-3 text-sm text-gray-900">
                                                <div className="flex items-center gap-2">
                                                    <button
                                                        onClick={() => handleDownload(report.id)}
                                                        className="text-blue-600 hover:text-blue-800"
                                                        title="Скачать отчет"
                                                    >
                                                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                                        </svg>
                                                    </button>
                                                    {report.name}
                                                </div>
                                            </td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{formatDate(report.createdAt)}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{formatTime(report.createdAt)}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{report.createdByUserName} {report.createdByUserSurname}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                    
                    {reports.length === 0 && !loading && (
                        <div className="text-center py-8 text-gray-500 flex-shrink-0">
                            Отчеты не найдены
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Reports; 