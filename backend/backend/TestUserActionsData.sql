-- Тестовые данные для таблицы действий
INSERT INTO action (id, code, name) VALUES 
(1, 'LOGIN', 'Вход в систему'),
(2, 'LOGOUT', 'Выход из системы'),
(3, 'DEVICE_VIEW', 'Просмотр устройства'),
(4, 'DEVICE_EDIT', 'Редактирование устройства'),
(5, 'REPORT_GENERATE', 'Генерация отчета'),
(6, 'SETTINGS_CHANGE', 'Изменение настроек'),
(7, 'USER_CREATE', 'Создание пользователя'),
(8, 'USER_DELETE', 'Удаление пользователя')
ON CONFLICT (id) DO NOTHING;

-- Тестовые данные для журнала действий пользователей
INSERT INTO "user_actions /* Таблица действий пользов" (id, action_id, date, user_id, description) VALUES 
(1, 1, NOW() - INTERVAL '1 hour', 1, 'Успешный вход в систему'),
(2, 3, NOW() - INTERVAL '2 hours', 1, 'Просмотр устройства ID: 123'),
(3, 4, NOW() - INTERVAL '3 hours', 2, 'Редактирование настроек устройства'),
(4, 5, NOW() - INTERVAL '4 hours', 1, 'Генерация отчета за месяц'),
(5, 2, NOW() - INTERVAL '5 hours', 3, 'Выход из системы'),
(6, 6, NOW() - INTERVAL '6 hours', 2, 'Изменение системных настроек'),
(7, 1, NOW() - INTERVAL '7 hours', 3, 'Вход в систему'),
(8, 7, NOW() - INTERVAL '8 hours', 1, 'Создание нового пользователя'),
(9, 3, NOW() - INTERVAL '9 hours', 2, 'Просмотр устройства ID: 456'),
(10, 8, NOW() - INTERVAL '10 hours', 1, 'Удаление пользователя ID: 5'),
(11, 1, NOW() - INTERVAL '1 day', 1, 'Вход в систему'),
(12, 5, NOW() - INTERVAL '2 days', 2, 'Генерация отчета за неделю'),
(13, 4, NOW() - INTERVAL '3 days', 3, 'Редактирование устройства ID: 789'),
(14, 6, NOW() - INTERVAL '4 days', 1, 'Изменение параметров системы'),
(15, 2, NOW() - INTERVAL '5 days', 2, 'Выход из системы')
ON CONFLICT (id) DO NOTHING; 