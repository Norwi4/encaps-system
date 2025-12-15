// Единая конфигурация для API URL
// В Docker окружении используем относительные пути (пустая строка)
// В development используем fallback на localhost:5000
const API_URL = (process.env.REACT_APP_API_URL || '').trim();

// Функция для получения полного URL для API запросов
export const getApiUrl = (path) => {
  // Убеждаемся, что path начинается с /
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  
  // Если API_URL не пустая строка, возвращаем полный URL
  if (API_URL && API_URL.trim() !== '') {
    // Убираем завершающий слэш из API_URL если есть
    const baseUrl = API_URL.endsWith('/') ? API_URL.slice(0, -1) : API_URL;
    return `${baseUrl}${normalizedPath}`;
  }
  // Возвращаем относительный путь для nginx прокси
  return normalizedPath;
};

// Функция для получения URL для SignalR
export const getSignalRUrl = () => {
  // Если API_URL не пустая строка, возвращаем полный URL
  if (API_URL && API_URL.trim() !== '') {
    const baseUrl = API_URL.endsWith('/') ? API_URL.slice(0, -1) : API_URL;
    return `${baseUrl}/notificationHub`;
  }
  // Возвращаем относительный путь для nginx прокси
  return '/notificationHub';
};

export default API_URL;

