# Многоэтапная сборка для React приложения
FROM node:18-alpine AS build

# Принимаем API URL как build argument (по умолчанию пустая строка для относительных путей)
ARG REACT_APP_API_URL
# Устанавливаем переменную окружения (если не передана, будет пустая строка)
ENV REACT_APP_API_URL=${REACT_APP_API_URL:-}

# Устанавливаем рабочую директорию
WORKDIR /app

# Копируем package.json и package-lock.json
COPY package*.json ./

# Устанавливаем зависимости (включая dev-зависимости для сборки)
RUN npm ci

# Копируем исходный код
COPY . .

# Собираем приложение
RUN npm run build

# Второй этап - nginx для раздачи статических файлов
FROM nginx:alpine

# Копируем собранное приложение
COPY --from=build /app/build /usr/share/nginx/html

# Копируем конфигурацию nginx
COPY nginx.conf /etc/nginx/nginx.conf

# Открываем порт 80
EXPOSE 80

# Запускаем nginx
CMD ["nginx", "-g", "daemon off;"]
