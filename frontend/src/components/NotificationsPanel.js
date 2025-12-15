import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const NotificationsPanel = () => {
  const [notifications, setNotifications] = useState([]);
  const [events, setEvents] = useState([]);
  const [activeTab, setActiveTab] = useState('events');
  const [connection, setConnection] = useState(null);

  useEffect(() => {
    // Создаем подключение к SignalR
    const newConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_URL}/notificationHub`, {
          withCredentials: true
        })
        .withAutomaticReconnect()
        .build();

    // Обработчик получения нового действия пользователя
    newConnection.on('ReceiveUserAction', (userAction) => {
      setNotifications(prev => {
        const newNotifications = [
          {
            id: userAction.id,
            type: 'action',
            icon: getActionIcon(userAction.actionCode),
            title: userAction.actionName,
            description: userAction.description,
            date: new Date(userAction.date),
            user: `${userAction.userName} ${userAction.userSurname}`.trim()
          },
          ...prev
        ];
        
        // Оставляем только последние 5 уведомлений
        return newNotifications.slice(0, 5);
      });
    });

    // Обработчик для нового события UserActionCreated
    newConnection.on('UserActionCreated', (userAction) => {
      // Получаем информацию о пользователе из localStorage
      const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
      const userName = currentUser.name && currentUser.surname 
        ? `${currentUser.name} ${currentUser.surname}` 
        : 'Пользователь';

      setNotifications(prev => {
        const newNotifications = [
          {
            id: userAction.id,
            type: 'action',
            icon: getActionIcon('SCAN_INTERVAL'), // Иконка для изменения времени опроса
            title: 'Изменение времени опроса',
            description: userAction.description,
            date: new Date(userAction.date),
            user: userName
          },
          ...prev
        ];
        
        // Оставляем только последние 5 уведомлений
        return newNotifications.slice(0, 5);
      });
    });

    // Запускаем подключение
    newConnection.start()
      .then(() => {
        console.log('SignalR Connected');
        newConnection.invoke('JoinNotificationGroup');
      })
      .catch(err => console.error('SignalR Connection Error: ', err));

    setConnection(newConnection);

    // Загружаем последние действия и события при монтировании
    loadRecentActions();
    loadRecentEvents();

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, []);

  const loadRecentActions = async () => {
    try {
      const response = await fetch(`${API_URL}/api/UserActions/log`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          periodType: 'new'
        })
      });

      if (response.ok) {
        const data = await response.json();
        const recentActions = data.userActions.slice(0, 5).map(action => ({
          id: action.id,
          type: 'action',
          icon: getActionIcon(action.actionCode),
          title: action.actionName,
          description: action.description,
          date: new Date(action.date),
          user: `${action.userName} ${action.userSurname}`.trim()
        }));
        
        setNotifications(recentActions);
      }
    } catch (error) {
      console.error('Error loading recent actions:', error);
    }
  };

  const loadRecentEvents = async () => {
    try {
      const response = await fetch(`${API_URL}/api/Event/log`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          periodType: 'new'
        })
      });

      if (response.ok) {
        const data = await response.json();
        const recentEvents = data.events.slice(0, 5).map(event => ({
          id: event.id,
          type: 'event',
          icon: getEventIcon(event.eventCode),
          title: event.eventName,
          description: event.eventDescription,
          date: new Date(event.date),
          object: event.objectName || 'Неизвестный объект'
        }));
        
        setEvents(recentEvents);
      }
    } catch (error) {
      console.error('Error loading recent events:', error);
    }
  };

  const getActionIcon = (actionCode) => {
    switch (actionCode) {
      case 'LOGIN':
        return '🔵'; // Синий круг для входа
      case 'LOGOUT':
        return '🔴'; // Красный круг для выхода
      case 'EDIT_COMMENT':
        return '🟡'; // Желтый круг для изменения комментария
      case 'EDIT_LAST_RECEIVE':
        return '🟢'; // Зеленый круг для изменения даты поверки
      case 'SCAN_INTERVAL':
        return '⏱️'; // Таймер для изменения времени опроса
      default:
        return '⚪'; // Белый круг для остальных действий
    }
  };

  const getEventIcon = (eventCode) => {
    switch (eventCode) {
      case 'ALARM':
        return '🚨'; // Сирена для тревоги
      case 'WARNING':
        return '⚠️'; // Предупреждение
      case 'INFO':
        return 'ℹ️'; // Информация
      case 'ERROR':
        return '❌'; // Ошибка
      default:
        return '📋'; // По умолчанию для событий
    }
  };

  const formatTime = (date) => {
    const now = new Date();
    const diff = now - date;
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'Только что';
    if (minutes < 60) return `${minutes} мин назад`;
    if (hours < 24) return `${hours} ч назад`;
    return `${days} дн назад`;
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
      <h3 className="text-black font-semibold mb-4">Последние оповещения:</h3>
      
      {/* Табы */}
      <div className="flex mb-4">
        <button
          onClick={() => setActiveTab('events')}
          className={`px-4 py-2 rounded-l-lg text-sm font-medium transition-colors ${
            activeTab === 'events'
              ? 'bg-gray-700 text-white'
              : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
          }`}
        >
          События
        </button>
        <button
          onClick={() => setActiveTab('actions')}
          className={`px-4 py-2 rounded-r-lg text-sm font-medium transition-colors ${
            activeTab === 'actions'
              ? 'bg-gray-700 text-white'
              : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
          }`}
        >
          Действия
        </button>
      </div>

             {/* Список уведомлений */}
       <div className="max-h-64 overflow-y-auto">
         {activeTab === 'actions' && notifications.length > 0 ? (
           notifications.map((notification) => (
             <div key={notification.id} className="flex items-start space-x-3 py-3 border-b border-gray-100 last:border-b-0">
               <div className="text-xl">{notification.icon}</div>
               <div className="flex-1 min-w-0">
                 <div className="text-sm text-gray-900 font-medium">{notification.title}</div>
                 <div className="text-xs text-gray-600 mt-1">{notification.description}</div>
                 <div className="text-xs text-gray-500 mt-1">
                   {notification.user} • {formatTime(notification.date)}
                 </div>
               </div>
             </div>
           ))
         ) : activeTab === 'actions' && notifications.length === 0 ? (
           <div className="text-center text-gray-500 py-8">
             <div className="text-2xl mb-2">👤</div>
             <div className="text-sm">Действия пользователей появятся здесь</div>
           </div>
         ) : activeTab === 'events' && events.length > 0 ? (
           events.map((event) => (
             <div key={event.id} className="flex items-start space-x-3 py-3 border-b border-gray-100 last:border-b-0">
               <div className="text-xl">{event.icon}</div>
               <div className="flex-1 min-w-0">
                 <div className="text-sm text-gray-900 font-medium">{event.title}</div>
                 <div className="text-xs text-gray-600 mt-1">{event.description}</div>
                 <div className="text-xs text-gray-500 mt-1">
                   {event.object} • {formatTime(event.date)}
                 </div>
               </div>
             </div>
           ))
         ) : (
           <div className="text-center text-gray-500 py-8">
             <div className="text-2xl mb-2">📋</div>
             <div className="text-sm">События появятся здесь</div>
           </div>
         )}
       </div>
    </div>
  );
};

export default NotificationsPanel; 