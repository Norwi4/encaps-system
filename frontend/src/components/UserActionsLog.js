import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5165';

const UserActionsLog = () => {
    const [userActions, setUserActions] = useState([]);
    const [users, setUsers] = useState([]);
    const [actions, setActions] = useState([]);
    const [loading, setLoading] = useState(false);
    const [filters, setFilters] = useState({
        userId: '',
        actionId: '',
        periodType: 'new',
        startDate: '',
        endDate: ''
    });

    useEffect(() => {
        fetchUsers();
        fetchActions();
        fetchUserActions();
    }, []);

    const fetchUsers = async () => {
        try {
            const response = await axios.get(`${API_URL}/api/UserActions/users`);
            setUsers(response.data);
        } catch (error) {
            console.error('Error fetching users:', error);
            // Mock data for development
            setUsers([
                { id: 1, name: 'Иван Иванов' },
                { id: 2, name: 'Петр Петров' },
                { id: 3, name: 'Анна Сидорова' }
            ]);
        }
    };

    const fetchActions = async () => {
        try {
            const response = await axios.get(`${API_URL}/api/UserActions/actions`);
            setActions(response.data);
        } catch (error) {
            console.error('Error fetching actions:', error);
            // Mock data for development
            setActions([
                { id: 1, code: 'LOGIN', name: 'Вход в систему' },
                { id: 2, code: 'LOGOUT', name: 'Выход из системы' },
                { id: 3, code: 'DEVICE_VIEW', name: 'Просмотр устройства' },
                { id: 4, code: 'DEVICE_EDIT', name: 'Редактирование устройства' },
                { id: 5, code: 'REPORT_GENERATE', name: 'Генерация отчета' }
            ]);
        }
    };

    const fetchUserActions = async () => {
        setLoading(true);
        try {
            const requestData = {
                userId: filters.userId || null,
                actionId: filters.actionId || null,
                periodType: filters.periodType,
                startDate: filters.startDate ? new Date(filters.startDate) : null,
                endDate: filters.endDate ? new Date(filters.endDate) : null
            };

            const response = await axios.post(`${API_URL}/api/UserActions/log`, requestData);
            setUserActions(response.data.userActions || []);
        } catch (error) {
            console.error('Error fetching user actions:', error);
            // Mock data for development
            setUserActions([
                {
                    id: 1,
                    actionCode: 'LOGIN',
                    actionName: 'Вход в систему',
                    date: '2024-04-10T12:55:00',
                    userName: 'Иван',
                    userSurname: 'Иванов',
                    description: 'Успешный вход в систему'
                },
                {
                    id: 2,
                    actionCode: 'DEVICE_VIEW',
                    actionName: 'Просмотр устройства',
                    date: '2024-04-10T12:45:00',
                    userName: 'Петр',
                    userSurname: 'Петров',
                    description: 'Просмотр устройства ID: 123'
                },
                {
                    id: 3,
                    actionCode: 'REPORT_GENERATE',
                    actionName: 'Генерация отчета',
                    date: '2024-04-10T12:35:00',
                    userName: 'Анна',
                    userSurname: 'Сидорова',
                    description: 'Генерация отчета за месяц'
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
        fetchUserActions();
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
                    
                    {/* User Filter */}
                    <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Пользователь:
                        </label>
                        <select
                            className="w-full p-2 border border-gray-300 rounded-md"
                            value={filters.userId}
                            onChange={(e) => handleFilterChange('userId', e.target.value)}
                        >
                            <option value="">Все пользователи</option>
                            {users.map(user => (
                                <option key={user.id} value={user.id}>
                                    {user.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Action Type Filter */}
                    <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Тип действия:
                        </label>
                        <select
                            className="w-full p-2 border border-gray-300 rounded-md"
                            value={filters.actionId}
                            onChange={(e) => handleFilterChange('actionId', e.target.value)}
                        >
                            <option value="">Все действия</option>
                            {actions.map(action => (
                                <option key={action.id} value={action.id}>
                                    {action.name}
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

                {/* Right Panel - User Actions Log Table */}
                <div className="flex-1 overflow-hidden flex flex-col">
                    <h3 className="text-lg font-semibold mb-4 flex-shrink-0">Журнал оповещений</h3>
                    
                    <div className="bg-white border border-gray-200 rounded-lg overflow-hidden flex-1">
                        <div className="overflow-x-auto overflow-y-auto h-full">
                            <table className="w-full min-w-[1200px]">
                                <thead className="bg-gray-50 sticky top-0">
                                    <tr>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-16">№</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-40">Дата и время</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-64">Пользователь</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-64">Действие</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 w-48">Код действия</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-700 min-w-[300px]">Описание</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-200">
                                    {userActions.map((action, index) => (
                                        <tr key={action.id} className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                            <td className="px-4 py-3 text-sm text-gray-900">№{index + 1}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{formatDate(action.date)}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{action.userName} {action.userSurname}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{action.actionName}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">{action.actionCode}</td>
                                            <td className="px-4 py-3 text-sm text-gray-900">
                                                {action.description}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                    
                    {userActions.length === 0 && !loading && (
                        <div className="text-center py-8 text-gray-500 flex-shrink-0">
                            Оповещения не найдены
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default UserActionsLog; 