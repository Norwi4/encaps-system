import React, { useState, useEffect } from "react";
import logo from "./icons/logo.svg";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { X, User, Key, Link, Edit } from "lucide-react";

const menu = [
  { label: "Профиль", icon: <User className="w-4 h-4" /> },
  { label: "Изменение пароля", icon: <Key className="w-4 h-4" /> },
  { label: "Объекты и счетчики", icon: <Link className="w-4 h-4" /> },
];

const API_URL = process.env.REACT_APP_API_URL;

function Settings() {
  const navigate = useNavigate();
  const [active, setActive] = useState(0);
  const [profile, setProfile] = useState({
    id: "",
    surname: "",
    name: "",
    patronymic: "",
    phone: "",
    email: ""
  });
  const [loading, setLoading] = useState(false);
  const [passwords, setPasswords] = useState({
    old: "",
    new1: "",
    new2: ""
  });
  const [pwLoading, setPwLoading] = useState(false);
  const [objects, setObjects] = useState([]);
  const [selectedObject, setSelectedObject] = useState(0);
  const [selectedDevice, setSelectedDevice] = useState(0);
  const [editingPhone, setEditingPhone] = useState(false);
  const [editingEmail, setEditingEmail] = useState(false);
  const [editingComment, setEditingComment] = useState(false);
  const [editingTrustedBefore, setEditingTrustedBefore] = useState(false);
  const [comment, setComment] = useState("");
  const [trustedBefore, setTrustedBefore] = useState("");
  
  // Новые состояния для редактирования устройств
  const [editingDeviceName, setEditingDeviceName] = useState(false);
  const [editingIpAddress, setEditingIpAddress] = useState(false);
  const [editingNetworkPort, setEditingNetworkPort] = useState(false);
  const [editingKoeffTrans, setEditingKoeffTrans] = useState(false);
  const [editingScanInterval, setEditingScanInterval] = useState(false);
  const [editingSortId, setEditingSortId] = useState(false);
  const [editingDevAddr, setEditingDevAddr] = useState(false);
  const [editingActive, setEditingActive] = useState(false);
  const [deviceName, setDeviceName] = useState("");
  const [ipAddress, setIpAddress] = useState("");
  const [networkPort, setNetworkPort] = useState("");
  const [koeffTrans, setKoeffTrans] = useState(1.0);
  const [deviceScanInterval, setDeviceScanInterval] = useState(10000);
  const [sortId, setSortId] = useState("");
  const [devAddr, setDevAddr] = useState("");
  const [deviceActive, setDeviceActive] = useState(true);
  const [deviceDetails, setDeviceDetails] = useState(null);

  useEffect(() => {
    const user = JSON.parse(localStorage.getItem("user"));
    if (user) {
      setProfile({
        id: user.id,
        surname: user.surname || "",
        name: user.name || "",
        patronymic: user.patronymic || "",
        phone: user.phone || "",
        email: user.email || ""
      });
    }
  }, []);



  useEffect(() => {
    if (active === 2) {
      axios.get(`${API_URL}/api/Object/all`).then(res => {
        setObjects(res.data);
        setSelectedObject(0);
        setSelectedDevice(0);
      });
    }
  }, [active]);

  // Обновляем комментарий и дату последней поверки при выборе устройства
  useEffect(() => {
    if (objects[selectedObject]?.devices?.[selectedDevice]) {
      const device = objects[selectedObject].devices[selectedDevice];
      setComment(device.comment || "");
              setTrustedBefore(device.trustedBefore ? new Date(device.trustedBefore).toISOString().slice(0, 16) : "");
      
      // Сбрасываем состояния редактирования
      setEditingDeviceName(false);
      setEditingIpAddress(false);
      setEditingNetworkPort(false);
      setEditingKoeffTrans(false);
      setEditingScanInterval(false);
      
      // Загружаем детальную информацию об устройстве
      loadDeviceDetails(device.id);
    }
  }, [selectedObject, selectedDevice, objects]);

  // Функция для загрузки детальной информации об устройстве
  const loadDeviceDetails = async (deviceId) => {
    try {
      const response = await axios.get(`${API_URL}/api/Device/details/${deviceId}`);
      const details = response.data;
      setDeviceDetails(details);
      setDeviceName(details.name || "");
      setIpAddress(details.ipAddress || "");
      setNetworkPort(details.networkPort || "");
      setKoeffTrans(details.koeffTrans || 1.0);
      setDeviceScanInterval(details.scanInterval || 10000);
      setSortId(details.sortId || "");
      setDevAddr(details.devAddr !== null && details.devAddr !== undefined ? details.devAddr.toString() : "");
      setDeviceActive(details.active !== undefined ? details.active : true);
    } catch (error) {
      console.error('Ошибка при загрузке деталей устройства:', error);
    }
  };

  const handleChange = e => {
    setProfile({ ...profile, [e.target.name]: e.target.value });
  };

  const handleSave = async () => {
    setLoading(true);
    try {
      await axios.post(`${API_URL}/api/User/profile/${profile.id}`, {
        Surname: profile.surname,
        Name: profile.name,
        Patronymic: profile.patronymic,
        Phone: profile.phone,
        Email: profile.email
      });
      // обновить localStorage
      const updatedUser = { ...JSON.parse(localStorage.getItem("user")), ...profile };
      localStorage.setItem("user", JSON.stringify(updatedUser));
      alert("Профиль успешно сохранён");
    } catch (err) {
      alert("Ошибка при сохранении профиля");
    }
    setLoading(false);
  };

  const handlePwChange = e => {
    setPasswords({ ...passwords, [e.target.name]: e.target.value });
  };

  const handlePwSave = async () => {
    if (!passwords.old || !passwords.new1 || !passwords.new2) {
      alert("Заполните все поля");
      return;
    }
    if (passwords.new1 !== passwords.new2) {
      alert("Новые пароли не совпадают");
      return;
    }
    setPwLoading(true);
    try {
      const user = JSON.parse(localStorage.getItem("user"));
      await axios.post(`${API_URL}/api/User/change-password/${user.id}`, {
        OldPassword: passwords.old,
        NewPassword: passwords.new1
      });
      alert("Пароль успешно изменён");
      setPasswords({ old: "", new1: "", new2: "" });
    } catch (err) {
      if (err.response && err.response.data && err.response.data.message) {
        alert(err.response.data.message);
      } else {
        alert("Ошибка при смене пароля");
      }
    }
    setPwLoading(false);
  };

  const handleCommentSave = async () => {
    if (!objects[selectedObject]?.devices?.[selectedDevice]) {
      alert("Выберите устройство");
      return;
    }

    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      await axios.put(`${API_URL}/api/Device/device/${deviceId}/update`, {
        Comment: comment
      });

      // Создаем запись о действии пользователя
      const user = JSON.parse(localStorage.getItem("user"));
      await axios.post(`${API_URL}/api/UserActions/create`, {
        UserId: user.id,
        ActionId: 1, // ID действия "Изменение комментария"
        Description: `Изменен комментарий устройства ${objects[selectedObject].devices[selectedDevice].name}`
      });

      // Обновляем данные в локальном состоянии
      const updatedObjects = [...objects];
      updatedObjects[selectedObject].devices[selectedDevice].comment = comment;
      setObjects(updatedObjects);

      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          comment: comment
        });
      }

      setEditingComment(false);
      alert("Комментарий успешно сохранен");
    } catch (err) {
      if (err.response && err.response.data && err.response.data.error) {
        alert(err.response.data.error);
      } else {
        alert("Ошибка при сохранении комментария");
      }
    }
  };

  const handleTrustedBeforeSave = async () => {
    if (!objects[selectedObject]?.devices?.[selectedDevice]) {
      alert("Выберите устройство");
      return;
    }

    if (!trustedBefore) {
      alert("Введите дату последней поверки");
      return;
    }

    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      const trustedBeforeDate = new Date(trustedBefore);
      
      await axios.put(`${API_URL}/api/Device/device/${deviceId}/update`, {
        TrustedBefore: trustedBeforeDate.toISOString()
      });

      // Создаем запись о действии пользователя
      const user = JSON.parse(localStorage.getItem("user"));
      await axios.post(`${API_URL}/api/UserActions/create`, {
        UserId: user.id,
        ActionId: 2, // ID действия "Изменение даты поверки"
        Description: `Изменена дата последней поверки устройства ${objects[selectedObject].devices[selectedDevice].name} на ${trustedBeforeDate.toLocaleDateString()}`
      });

      // Обновляем данные в локальном состоянии
      const updatedObjects = [...objects];
      updatedObjects[selectedObject].devices[selectedDevice].trustedBefore = trustedBeforeDate;
      setObjects(updatedObjects);

      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          trustedBefore: trustedBeforeDate
        });
      }

      setEditingTrustedBefore(false);
      alert("Дата последней поверки успешно сохранена");
    } catch (err) {
      if (err.response && err.response.data && err.response.data.error) {
        alert(err.response.data.error);
      } else {
        alert("Ошибка при сохранении даты последней поверки");
      }
    }
  };



  // Функции для сохранения изменений устройства
  const handleDeviceNameSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        name: deviceName
      });
      
      // Обновляем данные в локальном состоянии
      const updatedObjects = [...objects];
      updatedObjects[selectedObject].devices[selectedDevice].name = deviceName;
      setObjects(updatedObjects);
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          name: deviceName
        });
      }
      
      setEditingDeviceName(false);
      alert("Название устройства успешно обновлено");
    } catch (error) {
      alert("Ошибка при обновлении названия устройства");
    }
  };

  const handleIpAddressSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        ipAddress: ipAddress
      });
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          ipAddress: ipAddress
        });
      }
      
      setEditingIpAddress(false);
      alert("IP-адрес устройства успешно обновлен");
    } catch (error) {
      alert("Ошибка при обновлении IP-адреса устройства");
    }
  };

  const handleNetworkPortSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        networkPort: parseInt(networkPort)
      });
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          networkPort: parseInt(networkPort)
        });
      }
      
      setEditingNetworkPort(false);
      alert("Сетевой порт устройства успешно обновлен");
    } catch (error) {
      alert("Ошибка при обновлении сетевого порта устройства");
    }
  };

  const handleKoeffTransSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        koeffTrans: parseFloat(koeffTrans)
      });
      
      // Обновляем локальное состояние
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          koeffTrans: parseFloat(koeffTrans)
        });
      }
      
      setEditingKoeffTrans(false);
      alert("Коэффициент трансформации устройства успешно обновлен");
    } catch (error) {
      alert("Ошибка при обновлении коэффициента трансформации устройства");
    }
  };

  const handleDeviceScanIntervalSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      const user = JSON.parse(localStorage.getItem("user"));
      
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        scanInterval: parseInt(deviceScanInterval),
        userId: user?.id
      });
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          scanInterval: parseInt(deviceScanInterval)
        });
      }
      
      setEditingScanInterval(false);
      alert("Время опроса устройства успешно обновлено");
    } catch (error) {
      alert("Ошибка при обновлении времени опроса устройства");
    }
  };

  const handleSortIdSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        sortId: parseInt(sortId) || null
      });
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          sortId: parseInt(sortId) || null
        });
      }
      
      setEditingSortId(false);
      alert("ID сортировки устройства успешно обновлен");
    } catch (error) {
      alert("Ошибка при обновлении ID сортировки устройства");
    }
  };

  const handleDevAddrSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        devAddr: parseInt(devAddr) || null
      });
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          devAddr: parseInt(devAddr) || null
        });
      }
      
      setEditingDevAddr(false);
      alert("Сетевой адрес счетчика успешно обновлен");
    } catch (error) {
      alert("Ошибка при обновлении сетевого адреса счетчика");
    }
  };

  const handleActiveSave = async () => {
    try {
      const deviceId = objects[selectedObject].devices[selectedDevice].id;
      const user = JSON.parse(localStorage.getItem("user"));
      
      await axios.put(`${API_URL}/api/Device/edit`, {
        id: deviceId,
        active: deviceActive,
        userId: user?.id
      });
      
      // Обновляем локальное состояние deviceDetails
      if (deviceDetails) {
        setDeviceDetails({
          ...deviceDetails,
          active: deviceActive
        });
      }
      
      // Обновляем данные в локальном состоянии объектов
      const updatedObjects = [...objects];
      updatedObjects[selectedObject].devices[selectedDevice].active = deviceActive;
      setObjects(updatedObjects);
      
      setEditingActive(false);
      alert(`Устройство ${deviceActive ? 'включено' : 'выключено'}`);
    } catch (error) {
      console.error('Ошибка при изменении статуса устройства:', error);
      alert("Ошибка при изменении статуса устройства");
    }
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="w-full h-full">
        {/* Main Container */}
        <div className="relative bg-gray-100 h-full">

                      <div className="flex flex-col lg:flex-row h-screen">
              {/* Sidebar */}
              <div className="w-full lg:w-72 bg-white/70 backdrop-blur-sm border-r border-gray-300/30 p-6">
              <h2 className="text-xl font-semibold text-black mb-8">Настройки</h2>
              
              <div className="space-y-3">
          {menu.map((item, idx) => (
            <button
              key={item.label}
                    onClick={() => setActive(idx)}
                    className={`w-full flex items-center gap-3 px-4 py-3 rounded-full text-sm font-semibold transition-all ${
                active === idx
                        ? "bg-red-600 text-white"
                        : "bg-gray-300/30 text-black hover:bg-gray-300/50"
              }`}
            >
                    <span className={active === idx ? "text-white" : "text-red-600"}>{item.icon}</span>
              {item.label}
            </button>
          ))}
              
              {/* Кнопка выхода */}
              <div className="pt-8 border-t border-gray-200 mt-8 space-y-3">
                {/* Кнопка возврата на главную */}
                <button
                  onClick={() => navigate('/dashboard')}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-full text-sm font-semibold bg-blue-600 text-white hover:bg-blue-700 transition-all"
                >
                  <span className="text-white">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M3 12L5 10M5 10L12 3L19 10M5 10V20C5 20.5523 5.44772 21 6 21H9M19 10L21 12M19 10V20C19 20.5523 18.5523 21 18 21H15M9 21C9.55228 21 10 20.5523 10 20V16C10 15.4477 10.4477 15 11 15H13C13.5523 15 14 15.4477 14 16V20C14 20.5523 14.4477 21 15 21M9 21H15" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                  </span>
                  Вернуться на главную
                </button>
                
                {/* Кнопка выхода */}
                <button
                  onClick={async () => {
                    try {
                      const user = JSON.parse(localStorage.getItem('user'));
                      // Создаем запись о выходе из системы
                      await axios.post(`${API_URL}/api/UserActions/create`, {
                        UserId: user.id,
                        ActionId: 4, // ID действия "Выход из системы"
                        Description: `Пользователь ${user.name} ${user.surname} вышел из системы`
                      });
                    } catch (logError) {
                      console.error("Ошибка при логировании выхода:", logError);
                      // Не прерываем процесс выхода, если логирование не удалось
                    }
                    
                    localStorage.removeItem('user');
                    navigate('/');
                  }}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-full text-sm font-semibold bg-gray-300/30 text-black hover:bg-gray-300/50 transition-all"
                >
                  <span className="text-red-600">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                      <polyline points="16,17 21,12 16,7" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                      <line x1="21" y1="12" x2="9" y2="12" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                  </span>
                  Выход
                </button>
              </div>
              </div>
            </div>

                          {/* Content Area */}
              <div className="flex-1 bg-white/70 backdrop-blur-sm p-4 lg:p-6 overflow-y-auto">
        {/* Профиль */}
        {active === 0 && (
                <>
                  <h2 className="text-xl font-semibold text-black mb-8">Профиль</h2>
                  
                  <div className="space-y-6">
                    {/* Last Name */}
              <div>
                      <label className="block text-base text-black/50 mb-1">Фамилия</label>
                      <div className="text-base text-black">{profile.surname}</div>
              </div>

                    {/* First Name */}
              <div>
                      <label className="block text-base text-black/50 mb-1">Имя</label>
                      <div className="text-base text-black">{profile.name}</div>
              </div>

                    {/* Middle Name */}
              <div>
                      <label className="block text-base text-black/50 mb-1">Отчество</label>
                      <div className="text-base text-black">{profile.patronymic}</div>
              </div>

                    {/* Phone */}
              <div>
                      <label className="block text-base text-black/50 mb-2">Телефон</label>
                      <div className="relative">
                        {editingPhone ? (
                          <input
                            type="text"
                            name="phone"
                            value={profile.phone}
                            onChange={handleChange}
                            className="w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full text-base text-black focus:outline-none focus:ring-2 focus:ring-black/10"
                            onBlur={() => setEditingPhone(false)}
                            autoFocus
                          />
                        ) : (
                          <div className="w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full flex items-center justify-between">
                            <span className="text-base text-black">{profile.phone}</span>
                            <button 
                              onClick={() => setEditingPhone(true)}
                              className="text-black hover:text-gray-600 transition-colors"
                            >
                              <Edit className="w-4 h-4" />
                            </button>
                          </div>
                        )}
                      </div>
              </div>

                    {/* Email */}
              <div>
                      <label className="block text-base text-black/50 mb-2">Email</label>
                      <div className="relative">
                        {editingEmail ? (
                          <input
                            type="email"
                            name="email"
                            value={profile.email}
                            onChange={handleChange}
                            className="w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full text-base text-black focus:outline-none focus:ring-2 focus:ring-black/10"
                            onBlur={() => setEditingEmail(false)}
                            autoFocus
                          />
                        ) : (
                          <div className="w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full flex items-center justify-between">
                            <span className="text-base text-black">{profile.email}</span>
                            <button 
                              onClick={() => setEditingEmail(true)}
                              className="text-black hover:text-gray-600 transition-colors"
                            >
                              <Edit className="w-4 h-4" />
                            </button>
                          </div>
                        )}
                      </div>
              </div>

                    {/* Save Button */}
                    <div className="pt-8">
                      <button 
                        className="bg-black text-white px-8 py-2 rounded-full text-sm font-semibold hover:bg-gray-800 transition-colors"
                        onClick={handleSave} 
                        disabled={loading}
                      >
                        {loading ? "Сохранение..." : "Сохранить"}
                      </button>
            </div>
          </div>
                </>
        )}

              {/* Остальные вкладки */}
        {active === 1 && (
                <>
                  <h2 className="text-xl font-semibold text-black mb-8">Изменение пароля</h2>
                  <div className="space-y-6">
              <div>
                      <label className="block text-base text-black/50 mb-2">Старый пароль</label>
                      <input className="w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full text-base text-black" type="password" name="old" value={passwords.old} onChange={handlePwChange} />
              </div>
              <div>
                      <label className="block text-base text-black/50 mb-2">Новый пароль</label>
                      <input className="w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full text-base text-black" type="password" name="new1" value={passwords.new1} onChange={handlePwChange} />
              </div>
              <div>
                      <label className="block text-base text-black/50 mb-2">Повторить</label>
                      <input className={`w-full h-15 px-6 py-4 bg-white/5 border border-gray-200 rounded-full text-base text-black ${passwords.new1 && passwords.new2 && passwords.new1 !== passwords.new2 ? 'border-red-500' : ''}`} type="password" name="new2" value={passwords.new2} onChange={handlePwChange} />
                {passwords.new1 && passwords.new2 && passwords.new1 !== passwords.new2 && (
                  <span className="text-red-500 text-sm">Пароль не совпадает</span>
                )}
              </div>
                    <div className="pt-8">
                      <button className="bg-black text-white px-8 py-2 rounded-full text-sm font-semibold hover:bg-gray-800 transition-colors" onClick={handlePwSave} disabled={pwLoading}>
                        {pwLoading ? "Сохранение..." : "Сохранить"}
                      </button>
            </div>
          </div>
                </>
        )}

        {active === 2 && (
                <>
                  <h2 className="text-xl font-semibold text-black mb-8">Объекты и счетчики</h2>
                  <div className="flex flex-col lg:flex-row gap-8">
                    <div className="w-full lg:w-1/3 border-r border-gray-200 pr-4">
                      <h3 className="text-lg font-semibold text-black mb-4">Список объектов</h3>
              <ul className="space-y-2">
                {objects.map((obj, idx) => (
                  <li key={obj.id}>
                    <button
                              className={`block w-full text-left text-base ${selectedObject === idx ? 'text-red-600 font-semibold' : 'text-black'}`}
                      onClick={() => { setSelectedObject(idx); setSelectedDevice(0); }}
                    >
                      {obj.name || `Объект ${idx + 1}`}
                    </button>
                    {selectedObject === idx && obj.devices && obj.devices.length > 0 && (
                      <ul className="ml-4 text-black font-normal">
                        {obj.devices.map((dev, didx) => (
                          <li key={dev.id}>
                            <button
                                      className={`block w-full text-left text-sm ${selectedDevice === didx ? 'text-red-600' : 'text-black'}`}
                              onClick={() => setSelectedDevice(didx)}
                            >
                              {dev.name}
                            </button>
                          </li>
                        ))}
                      </ul>
                    )}
                  </li>
                ))}
              </ul>
            </div>
            <div className="flex-1 w-full">
              {objects[selectedObject] && (
                <>
                          <h3 className="text-lg font-semibold text-black mb-4">{objects[selectedObject].name || `Объект ${selectedObject + 1}`}</h3>
                          <div className="space-y-3">
                            <div className="text-base text-black/50">Адрес: <span className="text-black">{objects[selectedObject].place}</span></div>
                  {objects[selectedObject].devices && objects[selectedObject].devices[selectedDevice] && (
                    <>
                      {/* Название устройства */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        Название устройства: 
                        <div className="flex items-center gap-2">
                          {editingDeviceName ? (
                            <>
                              <input
                                type="text"
                                value={deviceName}
                                onChange={(e) => setDeviceName(e.target.value)}
                                className="border border-gray-200 rounded px-2 py-1 text-black"
                                placeholder="Введите название"
                              />
                              <button
                                onClick={handleDeviceNameSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingDeviceName(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.name || objects[selectedObject].devices[selectedDevice].name}</span>
                              <button
                                onClick={() => setEditingDeviceName(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* IP-адрес */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        IP-адрес: 
                        <div className="flex items-center gap-2">
                          {editingIpAddress ? (
                            <>
                              <input
                                type="text"
                                value={ipAddress}
                                onChange={(e) => setIpAddress(e.target.value)}
                                className="border border-gray-200 rounded px-2 py-1 text-black"
                                placeholder="192.168.1.100"
                              />
                              <button
                                onClick={handleIpAddressSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingIpAddress(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.ipAddress || 'Не указан'}</span>
                              <button
                                onClick={() => setEditingIpAddress(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Сетевой порт */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        Сетевой порт: 
                        <div className="flex items-center gap-2">
                          {editingNetworkPort ? (
                            <>
                              <input
                                type="number"
                                value={networkPort}
                                onChange={(e) => setNetworkPort(e.target.value)}
                                className="border border-gray-200 rounded px-2 py-1 text-black w-20"
                                placeholder="502"
                              />
                              <button
                                onClick={handleNetworkPortSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingNetworkPort(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.networkPort || 'Не указан'}</span>
                              <button
                                onClick={() => setEditingNetworkPort(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Коэффициент трансформации */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        Коэффициент трансформации: 
                        <div className="flex items-center gap-2">
                          {editingKoeffTrans ? (
                            <>
                              <input
                                type="number"
                                step="0.001"
                                value={koeffTrans}
                                onChange={(e) => setKoeffTrans(e.target.value)}
                                className="border border-gray-200 rounded px-2 py-1 text-black w-24"
                                placeholder="1.000"
                              />
                              <button
                                onClick={handleKoeffTransSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingKoeffTrans(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.koeffTrans || '1.000'}</span>
                              <button
                                onClick={() => setEditingKoeffTrans(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Время опроса */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        Время опроса (мс): 
                        <div className="flex items-center gap-2">
                          {editingScanInterval ? (
                            <>
                              <input
                                type="number"
                                min="10000"
                                max="300000"
                                value={deviceScanInterval}
                                onChange={(e) => setDeviceScanInterval(parseInt(e.target.value) || 10000)}
                                className="border border-gray-200 rounded px-2 py-1 text-black w-24"
                                placeholder="10000"
                              />
                              <button
                                onClick={handleDeviceScanIntervalSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingScanInterval(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.scanInterval || '10000'} мс</span>
                              <button
                                onClick={() => setEditingScanInterval(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* ID сортировки */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        ID сортировки: 
                        <div className="flex items-center gap-2">
                          {editingSortId ? (
                            <>
                              <input
                                type="number"
                                min="1"
                                value={sortId}
                                onChange={(e) => setSortId(e.target.value)}
                                className="border border-gray-200 rounded px-2 py-1 text-black w-24"
                                placeholder="1"
                              />
                              <button
                                onClick={handleSortIdSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingSortId(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.sortId || 'Не задан'}</span>
                              <button
                                onClick={() => setEditingSortId(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Сетевой адрес счетчика */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        Сетевой адрес счетчика: 
                        <div className="flex items-center gap-2">
                          {editingDevAddr ? (
                            <>
                              <input
                                type="number"
                                min="1"
                                max="255"
                                value={devAddr}
                                onChange={(e) => setDevAddr(e.target.value)}
                                className="border border-gray-200 rounded px-2 py-1 text-black w-24"
                                placeholder="1"
                              />
                              <button
                                onClick={handleDevAddrSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingDevAddr(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className="text-black">{deviceDetails?.devAddr !== null && deviceDetails?.devAddr !== undefined ? deviceDetails.devAddr : 'Не задан'}</span>
                              <button
                                onClick={() => setEditingDevAddr(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Статус устройства */}
                      <div className="text-base text-black/50 flex items-center justify-between">
                        Статус устройства: 
                        <div className="flex items-center gap-2">
                          {editingActive ? (
                            <>
                              <select
                                value={deviceActive}
                                onChange={(e) => setDeviceActive(e.target.value === 'true')}
                                className="border border-gray-200 rounded px-2 py-1 text-black"
                              >
                                <option value={true}>Включено</option>
                                <option value={false}>Выключено</option>
                              </select>
                              <button
                                onClick={handleActiveSave}
                                className="text-green-600 hover:text-green-800"
                              >
                                ✓
                              </button>
                              <button
                                onClick={() => setEditingActive(false)}
                                className="text-red-600 hover:text-red-800"
                              >
                                ✗
                              </button>
                            </>
                          ) : (
                            <>
                              <span className={`font-medium ${deviceDetails?.active ? 'text-green-600' : 'text-red-600'}`}>
                                {deviceDetails?.active ? 'Включено' : 'Выключено'}
                              </span>
                              <button
                                onClick={() => setEditingActive(true)}
                                className="text-blue-600 hover:text-blue-800 ml-2"
                              >
                                <Edit className="w-4 h-4" />
                              </button>
                            </>
                          )}
                        </div>
                      </div>

                                <div className="text-base text-black/50">Производитель прибора: <span className="text-black">{objects[selectedObject].devices[selectedDevice].vendor}</span></div>
                                <div className="text-base text-black/50">Модель: <span className="text-black">{objects[selectedObject].devices[selectedDevice].model}</span></div>
                                <div className="text-base text-black/50">Серийный номер: <span className="text-black">{objects[selectedObject].devices[selectedDevice].serialNo}</span></div>
                                <div className="text-base text-black/50">Тип связи с прибором: <span className="text-black">Bluetooth</span></div>
                                <div className="text-base text-black/50">Номер канала связи: <span className="text-black">№ {objects[selectedObject].devices[selectedDevice].channel}</span></div>
                                <div className="text-base text-black/50 flex items-center">Дата установки: <span className="text-black ml-2">{objects[selectedObject].devices[selectedDevice].installDate ? new Date(objects[selectedObject].devices[selectedDevice].installDate).toLocaleDateString() : '-'}</span></div>
                                <div className="text-base text-black/50">Комментарий:
                                  <div className="relative">
                                    {editingComment ? (
                                      <div className="mt-1">
                                        <textarea
                                          value={comment}
                                          onChange={(e) => setComment(e.target.value)}
                                          className="w-full border border-gray-200 rounded-lg p-3 bg-white/50 text-black resize-none"
                                          rows="3"
                                          placeholder="Введите комментарий"
                                        />
                                        <div className="flex gap-2 mt-2">
                                          <button
                                            onClick={handleCommentSave}
                                            className="bg-black text-white px-4 py-2 rounded-full text-sm font-semibold hover:bg-gray-800 transition-colors"
                                          >
                                            Сохранить
                                          </button>
                                          <button
                                            onClick={() => {
                                              setEditingComment(false);
                                              setComment(objects[selectedObject].devices[selectedDevice].comment || "");
                                            }}
                                            className="bg-gray-400 text-white px-4 py-2 rounded-full text-sm font-semibold hover:bg-gray-500 transition-colors"
                                          >
                                            Отмена
                                          </button>
                                        </div>
                                      </div>
                                    ) : (
                                      <div className="border border-gray-200 rounded-lg p-3 mt-1 bg-white/50 text-black flex items-center justify-between">
                                        <span>{objects[selectedObject].devices[selectedDevice].comment || "Комментарий отсутствует"}</span>
                                        <button 
                                          onClick={() => setEditingComment(true)}
                                          className="text-black hover:text-gray-600 transition-colors"
                                        >
                                          <Edit className="w-4 h-4" />
                                        </button>
                                      </div>
                                    )}
                                  </div>
                                </div>
                                
                                <div className="text-base text-black/50">Дата последней поверки:
                                  <div className="relative">
                                    {editingTrustedBefore ? (
                                      <div className="mt-1">
                                                                                   <input
                                             type="datetime-local"
                                             value={trustedBefore}
                                             onChange={(e) => setTrustedBefore(e.target.value)}
                                             className="w-full border border-gray-200 rounded-lg p-3 bg-white/50 text-black"
                                           />
                                        <div className="flex gap-2 mt-2">
                                          <button
                                            onClick={handleTrustedBeforeSave}
                                            className="bg-black text-white px-4 py-2 rounded-full text-sm font-semibold hover:bg-gray-800 transition-colors"
                                          >
                                            Сохранить
                                          </button>
                                          <button
                                            onClick={() => {
                                              setEditingTrustedBefore(false);
                                                      setTrustedBefore(objects[selectedObject].devices[selectedDevice].trustedBefore ? 
          new Date(objects[selectedObject].devices[selectedDevice].trustedBefore).toISOString().slice(0, 16) : "");
                                            }}
                                            className="bg-gray-400 text-white px-4 py-2 rounded-full text-sm font-semibold hover:bg-gray-500 transition-colors"
                                          >
                                            Отмена
                                          </button>
                                        </div>
                                      </div>
                                    ) : (
                                      <div className="border border-gray-200 rounded-lg p-3 mt-1 bg-white/50 text-black flex items-center justify-between">
                                                                                 <span>
                                           {objects[selectedObject].devices[selectedDevice].trustedBefore ? 
                                             new Date(objects[selectedObject].devices[selectedDevice].trustedBefore).toLocaleString() : 
                                             "Дата не указана"}
                                         </span>
                                        <button 
                                          onClick={() => setEditingTrustedBefore(true)}
                                          className="text-black hover:text-gray-600 transition-colors"
                                        >
                                          <Edit className="w-4 h-4" />
                                        </button>
                                      </div>
                                    )}
                                  </div>
                                </div>
                    </>
                  )}
                          </div>
                </>
              )}
            </div>
          </div>
                </>
        )}

            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Settings; 