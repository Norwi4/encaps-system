import React, { useState, useEffect } from "react";
import logo from "./icons/logo.svg";
import axios from "axios";
import { useNavigate } from "react-router-dom";

const API_URL = process.env.REACT_APP_API_URL;

function Login() {
  const [showPassword, setShowPassword] = useState(false);
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();

  // Проверяем, авторизован ли пользователь
  useEffect(() => {
    const user = localStorage.getItem('user');
    if (user) {
      try {
        const userData = JSON.parse(user);
        if (userData && userData.id) {
          navigate('/dashboard');
        }
      } catch (error) {
        localStorage.removeItem('user');
      }
    }
  }, [navigate]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(`${API_URL}/api/Auth/login`, { login, password });
      const user = response.data;
      
      // Сохраняем пользователя с информацией о роли
      localStorage.setItem("user", JSON.stringify(user));
      
      // Создаем запись о входе в систему
      try {
        await axios.post(`${API_URL}/api/UserActions/create`, {
          UserId: user.id,
          ActionId: 3, // ID действия "Вход в систему"
          Description: `Пользователь ${user.name} ${user.surname} вошел в систему`
        });
      } catch (logError) {
        console.error("Ошибка при логировании входа:", logError);
        // Не прерываем процесс входа, если логирование не удалось
      }
      
      navigate("/dashboard");
      alert("Успешная авторизация!");
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        alert(err.response.data.message);
      } else {
        alert("Ошибка соединения с сервером");
      }
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4 py-8">
      <div className="relative w-full max-w-[400px]">
        {/* Main card */}
        <div className="bg-white rounded-[10px] border border-[#e3e3e3] shadow-[0_2px_3.1px_0_rgba(2,2,2,0.10)] px-6 py-8 relative">
          {/* Logo */}
          <div className="flex justify-center mb-8">
            <img src={logo} alt="logo" className="w-32" />
          </div>

          {/* Title */}
          <h1 className="text-center text-[18px] font-semibold text-black mb-8">
            Авторизация
          </h1>

          <form onSubmit={handleSubmit}>
            {/* Login field */}
            <div className="mb-6">
              <label className="block text-[16px] text-black opacity-50 mb-2">
                Логин
              </label>
              <div className="relative">
                <input
                  type="text"
                  value={login}
                  onChange={e => setLogin(e.target.value)}
                  placeholder="Введите логин"
                  className="w-full h-[60px] px-6 rounded-full border border-[#eaeaea] bg-white/5 text-[16px] text-black placeholder:text-black placeholder:opacity-50 focus:outline-none focus:ring-2 focus:ring-black/10"
                  autoComplete="username"
                />
              </div>
            </div>

            {/* Password field */}
            <div className="mb-10">
              <label className="block text-[16px] text-black opacity-50 mb-2">
                Пароль
              </label>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  placeholder="Введите пароль"
                  className="w-full h-[60px] px-6 rounded-full border border-[#eaeaea] bg-white/5 text-[16px] text-black placeholder:text-black placeholder:opacity-50 focus:outline-none focus:ring-2 focus:ring-black/10"
                  autoComplete="current-password"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 transform -translate-y-1/2 text-black opacity-50 hover:opacity-70"
                >
                  {showPassword ? "Скрыть" : "Показать"}
                </button>
              </div>
            </div>

            {/* Submit button */}
            <button
              type="submit"
              className="w-full h-[60px] bg-black text-white rounded-full text-[16px] font-semibold hover:bg-gray-800 transition-colors"
            >
              Войти
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

export default Login; 