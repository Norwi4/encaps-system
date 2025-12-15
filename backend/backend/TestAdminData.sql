-- Тестовые данные для администрирования пользователей

-- Добавление профилей пользователей
INSERT INTO "Profiles" ("Name", "Surname", "Patronymic", "Phone") VALUES
('Сергей', 'Прохорцев', 'Сергеевич', '+7 876 766 67 67'),
('Анна', 'Иванова', 'Петровна', '+7 912 345 67 89'),
('Михаил', 'Сидоров', 'Александрович', '+7 987 654 32 10'),
('Елена', 'Козлова', 'Владимировна', '+7 911 222 33 44'),
('Дмитрий', 'Новиков', 'Игоревич', '+7 933 444 55 66');

-- Добавление пользователей
INSERT INTO "Users" ("Login", "Password", "Email", "ProfileId") VALUES
('serg1122', 'dsfyfdsf345345', 'sp@gmail.com', 1),
('anna_ivanova', 'password123', 'anna.ivanova@example.com', 2),
('mikhail_sidorov', 'securepass456', 'mikhail.sidorov@example.com', 3),
('elena_kozlova', 'elena2024', 'elena.kozlova@example.com', 4),
('dmitry_novikov', 'dmitry_pass', 'dmitry.novikov@example.com', 5); 