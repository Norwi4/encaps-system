import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

const ProtectedRoute = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const checkAuth = () => {
      const user = localStorage.getItem('user');
      if (!user) {
        navigate('/');
        return;
      }
      
      try {
        const userData = JSON.parse(user);
        if (!userData || !userData.id) {
          localStorage.removeItem('user');
          navigate('/');
          return;
        }
        
        setIsAuthenticated(true);
      } catch (error) {
        console.error('Ошибка при парсинге данных пользователя:', error);
        localStorage.removeItem('user');
        navigate('/');
        return;
      }
      
      setIsLoading(false);
    };

    checkAuth();
  }, [navigate]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-black mx-auto mb-4"></div>
          <p className="text-gray-600">Загрузка...</p>
        </div>
      </div>
    );
  }

  return isAuthenticated ? children : null;
};

export default ProtectedRoute; 