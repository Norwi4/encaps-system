-- Обновление таблицы report для добавления новых полей
ALTER TABLE report 
ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
ADD COLUMN IF NOT EXISTS created_by_user_id BIGINT;

-- Добавление внешнего ключа для связи с таблицей пользователей
ALTER TABLE report 
ADD CONSTRAINT IF NOT EXISTS report_created_by_user_id_fkey 
FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL;

-- Обновление существующих записей (если есть)
UPDATE report 
SET created_at = NOW() 
WHERE created_at IS NULL;

-- Тестовые данные для отчетов
INSERT INTO report (type, name, path, size, created_at, created_by_user_id) VALUES 
('data_visualization', 'Отчет по данным 10.04.24', 'report_20240410_126500.xlsx', 1024, NOW() - INTERVAL '1 hour', 1),
('data_visualization', 'Отчет по данным 09.04.24', 'report_20240409_143000.xlsx', 2048, NOW() - INTERVAL '2 hours', 1),
('data_visualization', 'Отчет по данным 08.04.24', 'report_20240408_091500.xlsx', 1536, NOW() - INTERVAL '3 hours', 2)
ON CONFLICT (id) DO NOTHING; 