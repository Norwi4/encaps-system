-- Добавление действий для логирования
INSERT INTO actions (code, name) VALUES 
('EDIT_COMMENT', 'Изменение комментария устройства'),
('EDIT_LAST_RECEIVE', 'Изменение даты поверки (TrustedBefore)'),
('LOGIN', 'Вход в систему'),
('LOGOUT', 'Выход из системы')
ON CONFLICT (code) DO NOTHING; 