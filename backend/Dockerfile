# Многоэтапная сборка для .NET 8 приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Устанавливаем рабочую директорию
WORKDIR /src

# Копируем файлы проекта
COPY ["backend/backend/backend.csproj", "backend/backend/"]

# Восстанавливаем зависимости
RUN dotnet restore "backend/backend/backend.csproj"

# Копируем весь исходный код
COPY . .

# Собираем приложение
RUN dotnet build "backend/backend/backend.csproj" -c Release -o /app/build

# Публикуем приложение
RUN dotnet publish "backend/backend/backend.csproj" -c Release -o /app/publish

# Второй этап - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Устанавливаем рабочую директорию
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Создаем директорию для отчетов
RUN mkdir -p /app/Reports

# Открываем порт 5000
EXPOSE 5000

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Запускаем приложение
ENTRYPOINT ["dotnet", "backend.dll"]