import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import * as signalR from '@microsoft/signalr';
import icon1 from "./icons/1.svg";
import icon2 from "./icons/2.svg";
import icon3 from "./icons/3.svg";
import icon4 from "./icons/4.svg";
import icon5 from "./icons/5.svg";
import icon6 from "./icons/6.svg";
import icon7 from "./icons/7.svg";
import settingsIcon from "./icons/settings.svg";
import profileIcon from "./icons/profile.svg";
import DeviceAccounting from "./components/DeviceAccounting";
import DataVisualization from "./components/DataVisualization";
import Administration from "./components/Administration";
import EventLog from "./components/EventLog";
import AlarmLog from "./components/AlarmLog";
import UserActionsLog from "./components/UserActionsLog";
import Reports from "./components/Reports";
import NotificationsPanel from "./components/NotificationsPanel";

const API_URL = process.env.REACT_APP_API_URL;

function Dashboard() {
  const [metrics, setMetrics] = useState({
    previousMonthConsumption: [],
    monthlyConsumption: [],
    dailyConsumption: [],
    previousDayConsumption: []
  });
  const [activeMenuItem, setActiveMenuItem] = useState(1); // 1 = Приборы учёта, 2 = Визуализация данных
  const [user, setUser] = useState(null);
  const [isAdmin, setIsAdmin] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    // Загружаем метрики дашборда
    axios.get(`${API_URL}/api/Dashboard/metrics`).then(res => {
      console.log('=== Dashboard Metrics Response ===');
      console.log('Full response:', res.data);
      console.log('Previous month consumption:', res.data.previousMonthConsumption);
      console.log('Monthly consumption:', res.data.monthlyConsumption);
      console.log('Daily consumption:', res.data.dailyConsumption);
      console.log('Daily consumption gas data:', res.data.dailyConsumption?.map(s => ({ name: s.siteName, gas: s.gasConsumption })));
      console.log('Previous day consumption:', res.data.previousDayConsumption);
      console.log('================================');
      setMetrics(res.data);
    }).catch(error => {
      console.error('Error fetching dashboard metrics:', error);
    });
    
    const userData = JSON.parse(localStorage.getItem("user"));
    setUser(userData);
    setIsAdmin(userData?.isAdmin || false);

    // Создаем подключение к SignalR
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/notificationHub`, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    // Обработчик обновления расхода за сегодня
    newConnection.on('UpdateConsumptionToday', (dailyData) => {
      console.log('Received consumption today update:', dailyData);
      setMetrics(prev => ({
        ...prev,
        dailyConsumption: dailyData
      }));
    });

    // Запускаем подключение
    newConnection.start()
      .then(() => {
        console.log('SignalR Connected for Dashboard');
        newConnection.invoke('JoinNotificationGroup');
      })
      .catch(err => console.error('SignalR Connection Error: ', err));

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, []);

  const menuItems = [
    { id: 1, text: "Приборы учёта", icon: icon1, active: activeMenuItem === 1 },
    { id: 2, text: "Визуализация данных", icon: icon2, active: activeMenuItem === 2 },
    { id: 3, text: "Журнал событий", icon: icon3, active: activeMenuItem === 3 },
    { id: 4, text: "Журнал оповещений", icon: icon4, active: activeMenuItem === 4 },
    { id: 5, text: "Отчеты", icon: icon5, active: activeMenuItem === 5 },
    { id: 6, text: "Аварии", icon: icon6, active: activeMenuItem === 6 },
    // Показываем вкладку администрирования только администраторам
    ...(isAdmin ? [{ id: 7, text: "Администрирование", icon: icon7, active: activeMenuItem === 7 }] : [])
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="flex-responsive">
        {/* Left Sidebar */}
        <div className="w-full xl:w-80 xl:h-[950px] bg-white/70 rounded-[20px] border border-neutral-400/30 m-4 xl:m-[30px] xl:mr-4">
          <div className="p-6">
            {/* Logo */}
            <div className="flex items-center gap-6 mb-6">
              <div className="w-36 h-8 relative">
                <div data-svg-wrapper className="left-0 top-0 absolute">
                  <svg width="152" height="34" viewBox="0 0 152 34" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M62.6296 1.29934C63.7452 0.853123 65.4025 0.343163 66.3268 0.183801H66.295C67.2193 0.0244387 68.9085 -0.0393062 70.0559 0.0244387C71.2033 0.120056 72.6376 0.343163 73.2432 0.502526C73.8487 0.69376 74.9005 1.07623 76.781 1.90491L76.7491 4.83718C76.7491 6.59016 76.6216 7.80132 76.4304 7.80132C76.2392 7.80132 75.7292 7.54634 75.2192 7.22761C74.7412 6.90889 73.7212 6.36706 72.9244 6.04833C72.0001 5.69774 70.5659 5.47463 68.9404 5.47463C66.7093 5.44276 66.1356 5.53837 64.3189 6.4308C62.9165 7.10012 61.7053 8.02443 60.5579 9.29932C59.5061 10.4467 58.6137 11.8172 58.1994 12.9009C57.6894 14.3033 57.5938 15.2276 57.6894 17.3631C57.785 19.5623 57.9762 20.3909 58.7093 21.8252C59.1874 22.7814 60.2073 24.1838 60.9085 24.9168C61.6416 25.6818 62.9484 26.6379 63.8408 27.0842C64.7013 27.5304 66.2312 28.0085 67.1874 28.1997C68.2392 28.3591 69.6416 28.3591 70.6934 28.1997C71.6495 28.0403 72.7332 27.8172 73.0838 27.6897C73.4344 27.5622 74.4224 27.0204 75.3149 26.4786C76.1754 25.9367 77.3866 24.8531 78.024 24.0563C78.9165 22.9407 79.6495 21.1877 81.3388 16.0882C82.5499 12.4866 83.7929 8.94873 84.1435 8.18379C84.5897 7.16387 85.036 6.68578 86.0559 6.2077C86.8846 5.82523 88.1276 5.57025 89.1794 5.57025C90.3905 5.57025 91.3786 5.79335 92.3666 6.27144C93.6097 6.90889 93.8965 7.25949 94.5658 8.91685C94.9802 9.96865 96.5738 14.7176 98.1037 19.4348C99.6336 24.1519 100.781 28.1359 100.685 28.2634C100.59 28.4228 97.9124 28.4865 89.02 28.3591L88.8607 23.4188H91.2511C92.6216 23.4188 93.6415 23.2913 93.6415 23.1001C93.6415 22.9407 92.7172 20.1041 91.5698 16.7894C90.4224 13.5065 89.4025 10.7655 89.3069 10.6698C89.1794 10.5742 89.02 10.638 88.9563 10.8292C88.8607 10.9886 87.4901 15.0045 85.8965 19.7535C83.0599 28.0722 82.9324 28.3272 82.0081 28.5822C81.4981 28.7415 80.3188 29.4108 79.4583 30.0802C78.5659 30.7495 77.291 31.61 76.5898 31.9606C75.8886 32.3112 74.4543 32.8531 73.4025 33.1399C72.1914 33.5224 70.5021 33.7136 68.781 33.6818C66.9962 33.6818 65.3707 33.4586 64.0001 33.0124C62.8527 32.6618 61.0679 31.8332 60.0161 31.132C58.9643 30.4626 57.3707 29.124 56.5101 28.1359C55.6177 27.1479 54.534 25.5861 54.0559 24.6618C53.5779 23.7057 52.9723 22.0802 52.7173 21.0284C52.4304 19.9766 52.2073 18.0961 52.2073 16.885C52.2073 15.6738 52.4304 13.8252 52.6854 12.8053C52.9404 11.8172 53.5778 10.1599 54.0878 9.17183C54.5978 8.15191 55.5858 6.68578 56.2551 5.88897C56.9563 5.09216 58.2312 3.91288 59.0918 3.27543C59.9523 2.63798 61.546 1.74555 62.6296 1.29934Z" fill="black"/>
                  <path d="M26.0719 7.80132C26.4225 7.132 27.2193 6.39893 28.048 5.95272C29.0679 5.44276 29.9604 5.25152 31.5221 5.25152C32.8608 5.25152 33.9763 5.44276 34.7094 5.82523C35.3787 6.14395 36.1117 6.87702 36.4942 7.64196C36.8767 8.34315 38.056 11.7854 39.1078 15.2913C40.1595 18.7973 41.1795 22.0483 41.3707 22.5264C41.5938 23.1001 41.9444 23.4188 42.3587 23.4188C42.7093 23.4188 43.0599 23.1957 43.1556 22.9407C43.2512 22.6858 43.3149 18.5742 43.3149 5.25152L48.2551 5.41088V15.3551C48.2551 24.7893 48.2233 25.3312 47.6177 26.1599C47.2671 26.6061 46.5659 27.3073 46.0878 27.6897C45.4185 28.1678 44.5898 28.3909 43.0599 28.4865C41.3707 28.5822 40.6376 28.4547 39.554 27.9128C38.8209 27.5622 37.8647 26.7654 37.4504 26.1917C36.9723 25.5862 35.8249 22.367 34.5181 18.1599C33.3388 14.3033 32.1914 10.9248 31.9683 10.6698C31.7133 10.383 31.3627 10.2874 31.044 10.4149C30.6297 10.638 30.5659 11.9129 30.4066 28.3591L25.4663 28.5184V18.7017C25.4663 9.74554 25.4982 8.82124 26.0719 7.80132Z" fill="black"/>
                  <path d="M1.40264 7.83319C1.81698 7.22761 2.70941 6.52642 3.50622 6.17582C4.74924 5.63399 5.83291 5.57025 21.6416 5.57025V10.3511H14.2154C8.1596 10.3511 6.69346 10.4467 6.08789 10.8292C5.57793 11.1798 5.38669 11.626 5.38669 12.4228C5.38669 13.2196 5.57793 13.6658 6.08789 14.0164C6.66159 14.3989 7.84087 14.4945 15.9046 14.4945V19.5941H11.0281C7.55402 19.5941 6.05602 19.7216 5.76916 19.9766C5.54606 20.1997 5.38669 20.8053 5.38669 21.3471C5.38669 21.8571 5.64167 22.5583 5.9604 22.877C6.43848 23.3869 7.2353 23.4507 21.6416 23.4188L21.4823 28.3591H13.0361C5.83291 28.3591 4.46239 28.2953 3.63371 27.8491C3.09188 27.5622 2.19945 26.7973 1.62574 26.1599C1.08391 25.5224 0.446462 24.4069 0.255228 23.6419C0.0639928 22.9089 -0.0316246 21.5065 0.0639928 20.5503C0.127738 19.5941 0.41459 18.3511 0.701442 17.8411C1.14766 16.9806 1.14766 16.7894 0.701442 15.9288C0.41459 15.387 0.127738 14.1758 0.0321203 13.2196C-0.0634971 12.2316 0.0639928 10.9248 0.2871 10.1918C0.510207 9.49056 1.02017 8.43877 1.40264 7.83319Z" fill="black"/>
                  <path d="M101.004 10.3511V7.96068V5.57025H110.98C119.873 5.57025 121.052 5.63399 122.199 6.17582C122.9 6.46268 123.857 7.19574 124.367 7.73757C124.845 8.31128 125.355 9.29932 125.546 9.93677C125.705 10.6061 125.865 11.8491 125.865 12.7415C125.865 13.634 125.578 14.877 125.227 15.6101C124.908 16.3113 124.367 17.2037 124.048 17.5543C123.729 17.9368 122.964 18.5105 122.359 18.8292C121.434 19.3073 120.287 19.4029 110.566 19.5941L109.928 20.4547C109.355 21.2196 109.259 21.8252 109.291 28.5184L104.351 28.3591V23.5782C104.383 19.4985 104.478 18.6061 104.988 17.5543C105.339 16.8531 106.104 15.9288 106.709 15.4826C107.761 14.6858 107.952 14.6539 113.817 14.4945C118.693 14.367 119.904 14.2396 120.128 13.8571C120.287 13.6021 120.446 12.9009 120.446 12.3272C120.446 11.626 120.223 11.1479 119.745 10.8292C119.139 10.4149 117.514 10.3511 101.004 10.3511Z" fill="black"/>
                  <path d="M130.709 8.34315C131.347 7.38698 132.207 6.59017 133.036 6.2077C134.183 5.63399 135.076 5.57025 151.044 5.57025V10.3511H143.554C136.478 10.3511 136.032 10.383 135.426 10.9886C135.076 11.3392 134.789 12.0404 134.789 12.5822C134.789 13.2196 135.033 13.6977 135.522 14.0164C136.064 14.3989 137.371 14.4945 141.801 14.4945C146.454 14.4945 147.602 14.5902 148.653 15.0682C149.355 15.387 150.215 16.0563 150.598 16.5981C150.98 17.1081 151.426 18.0324 151.649 18.638C151.841 19.2435 152 20.4547 152 21.3471C152 22.2077 151.809 23.5144 151.586 24.2156C151.394 24.9168 150.693 26.0005 150.088 26.6379C149.291 27.4348 148.494 27.881 147.315 28.1359C146.135 28.3909 140.781 28.5184 114.072 28.5184V23.4188H129.785C138.39 23.4188 145.626 23.2913 145.849 23.1638C146.04 23.0364 146.359 22.5901 146.518 22.1439C146.741 21.5702 146.741 21.124 146.072 19.7535L140.207 19.5941C135.904 19.4348 134.024 19.2754 133.195 18.9248C132.59 18.638 131.633 17.9368 131.092 17.3312C130.486 16.6619 129.976 15.5782 129.689 14.4945C129.466 13.5065 129.371 12.136 129.498 11.3073C129.594 10.4786 130.135 9.20371 130.709 8.34315Z" fill="black"/>
                  </svg>
                </div>
                <div data-svg-wrapper className="left-[61.45px] top-[9.36px] absolute">
                  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M2.58589 2.81722C3.25522 2.11602 4.37075 1.28734 5.07195 0.968614C5.77314 0.681762 6.92055 0.39491 8.89664 0.363037L8.00421 1.19172C7.52612 1.63794 7.01617 2.33913 6.82493 2.78534C6.6337 3.23156 6.50621 4.06024 6.50621 4.63395C6.50621 5.17578 6.79306 6.13195 7.14365 6.70566C7.49425 7.24749 8.19545 7.94868 8.73728 8.20366C9.27911 8.68175 10.2034 8.45864 10.809 8.68175C11.4146 8.68175 12.3389 8.45864 12.8807 8.20366C13.3907 7.94868 14.0919 7.27936 14.3787 6.7694C14.6974 6.22757 15.048 5.81323 15.1755 5.81323C15.3349 5.81323 15.4305 6.29131 15.4624 6.86502C15.4942 7.40685 15.4624 8.29928 15.4305 8.84111C15.3986 9.38294 15.1436 10.371 14.8249 11.0403C14.5381 11.7415 13.7413 12.8252 13.0082 13.4307C12.307 14.0682 11.0321 14.7694 10.1397 15.0244C9.24724 15.2793 7.97234 15.4068 7.14365 15.3112C6.34684 15.2156 5.23131 14.8969 4.68948 14.61C4.11577 14.3232 3.12773 13.5582 2.52215 12.9208C1.8847 12.2515 1.18351 11.0722 0.928526 10.2754C0.641674 9.47856 0.450439 8.42677 0.450439 7.88494C0.450439 7.34311 0.641674 6.29131 0.928526 5.4945C1.18351 4.69769 1.91657 3.48654 2.58589 2.81722Z" fill="#6C2126"/>
                  </svg>
                </div>
                <div data-svg-wrapper className="left-[67.51px] top-[9.49px] absolute">
                  <svg width="10" height="9" viewBox="0 0 10 9" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M0.824828 2.78536C1.01606 2.33914 1.52602 1.60608 2.00411 1.15986C2.7053 0.522414 3.08777 0.394925 3.85271 0.554287C4.36267 0.649904 4.87263 0.809267 4.96825 0.873011C5.06387 0.968629 4.74514 1.38297 4.26705 1.82919C3.69335 2.33914 3.37462 2.94472 3.37462 3.48655C3.37462 3.99651 3.59773 4.69771 3.85271 5.08018C4.20331 5.62201 4.64952 5.81324 5.54195 5.81324C6.21127 5.81324 6.97621 5.55826 7.29494 5.23954C7.61366 4.95269 7.86864 4.50647 7.83677 4.28336C7.83677 4.09213 7.99613 3.9009 8.15549 3.9009C8.34673 3.9009 8.63358 4.21962 8.82482 4.60209C9.14354 5.20767 9.0798 5.55826 8.50609 6.51444C8.15549 7.18376 7.39055 7.9487 6.8806 8.20368C6.37064 8.45866 5.41446 8.68176 4.80889 8.68176C4.20331 8.68176 3.27901 8.45866 2.73718 8.20368C2.22722 7.9487 1.49415 7.2475 1.14355 6.6738C0.792956 6.13197 0.506104 5.17579 0.506104 4.60209C0.506104 4.06026 0.665466 3.23157 0.824828 2.78536Z" fill="#CC0C20"/>
                  </svg>
                </div>
              </div>
              <div className="w-28 justify-start text-black text-xs font-semibold font-montserrat uppercase leading-3">
                Система управления
              </div>
            </div>
            
            {/* Navigation Menu */}
            <nav className="space-y-2">
              {menuItems.map((item) => (
                <div
                  key={item.id}
                  className={`w-64 h-11 px-4 py-1 rounded-[100px] inline-flex justify-start items-center gap-2.5 cursor-pointer ${
                    item.active 
                      ? 'bg-red-700' 
                      : 'bg-zinc-300/30'
                  }`}
                  onClick={() => setActiveMenuItem(item.id)}
                >
                  <div data-svg-wrapper>
                    <img 
                      src={item.icon} 
                      alt={item.text} 
                      width="15" 
                      height="15"
                      className={`${
                        item.active 
                          ? 'filter brightness-0 invert' // Белая иконка для активного состояния
                          : 'filter brightness-0' // Черная иконка для неактивного состояния
                      }`}
                    />
                  </div>
                  <div className={`justify-start text-sm font-semibold font-open-sans ${
                    item.active ? 'text-white' : 'text-black'
                  }`}>
                    {item.text}
                  </div>
                </div>
              ))}
            </nav>
            
            {/* Notifications Panel */}
            <div className="mt-6">
              <NotificationsPanel />
            </div>
          </div>
        </div>

        {/* Main Content */}
        <main className="flex-1 w-full">
          {/* Header */}
          <div className="w-full mt-4 xl:mt-[30px]">
            <div className="flex flex-col lg:flex-row items-stretch gap-4">
              {/* 1. Электроснабжение Block */}
              <div className="flex-1 bg-white/70 rounded-[20px] border border-neutral-400/30 px-4 lg:px-6 py-4 flex flex-col">
                <div className="flex flex-col items-center flex-1">
                  <div className="text-black text-sm font-semibold font-open-sans mb-3">Электроснабжение</div>
                  <div className="flex flex-col lg:flex-row items-center gap-4 lg:gap-6">
                    {/* Расход за предыдущий месяц */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за предыдущий месяц</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.previousMonthConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект за предыдущий месяц:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.previousMonthConsumption?.map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: ⚡{site.electricityConsumption}
                          </option>
                        ))}
                      </select>
                    </div>
                    
                    <div data-svg-wrapper>
                      <svg width="2" height="40" viewBox="0 0 2 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M1 1L1 39" stroke="black" strokeOpacity="0.1" strokeLinecap="round"/>
                      </svg>
                    </div>
                    
                    {/* Расход за месяц */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за месяц</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.monthlyConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект за месяц:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.monthlyConsumption?.map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: ⚡{site.electricityConsumption}
                          </option>
                        ))}
                      </select>
                    </div>
                    
                    <div data-svg-wrapper>
                      <svg width="2" height="40" viewBox="0 0 2 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M1 1L1 39" stroke="black" strokeOpacity="0.1" strokeLinecap="round"/>
                      </svg>
                    </div>
                    
                    {/* Расход за день */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за день</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.dailyConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект за день:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.dailyConsumption?.map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: ⚡{site.electricityConsumption}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div data-svg-wrapper>
                      <svg width="2" height="40" viewBox="0 0 2 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M1 1L1 39" stroke="black" strokeOpacity="0.1" strokeLinecap="round"/>
                      </svg>
                    </div>

                    {/* Расход за предыдущий день */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за вчера</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.previousDayConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект за вчера:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.previousDayConsumption?.map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: ⚡{site.electricityConsumption}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>
                </div>
              </div>

              {/* 2. Газоснабжение Block */}
              <div className="flex-1 bg-white/70 rounded-[20px] border border-neutral-400/30 px-4 lg:px-6 py-4 flex flex-col">
                <div className="flex flex-col items-center flex-1">
                  <div className="text-black text-sm font-semibold font-open-sans mb-3">Газоснабжение</div>
                  <div className="flex flex-col lg:flex-row items-center gap-4 lg:gap-6">
                    {/* Расход за предыдущий месяц */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за предыдущий месяц</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.previousMonthConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект газа за предыдущий месяц:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.previousMonthConsumption?.filter(site => ['КВТ-Юг', 'КВТ-Восток', 'КВТ-Север'].includes(site.siteName)).map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: 🔥{site.gasConsumption}
                          </option>
                        ))}
                      </select>
                    </div>
                    
                    <div data-svg-wrapper>
                      <svg width="2" height="40" viewBox="0 0 2 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M1 1L1 39" stroke="black" strokeOpacity="0.1" strokeLinecap="round"/>
                      </svg>
                    </div>
                    
                    {/* Расход за месяц */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за месяц</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.monthlyConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект газа за месяц:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.monthlyConsumption?.filter(site => ['КВТ-Юг', 'КВТ-Восток', 'КВТ-Север'].includes(site.siteName)).map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: 🔥{site.gasConsumption}
                          </option>
                        ))}
                      </select>
                    </div>
                    
                    <div data-svg-wrapper>
                      <svg width="2" height="40" viewBox="0 0 2 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M1 1L1 39" stroke="black" strokeOpacity="0.1" strokeLinecap="round"/>
                      </svg>
                    </div>
                    
                    {/* Расход за день */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за день</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.dailyConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект газа за день:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.dailyConsumption?.filter(site => ['КВТ-Юг', 'КВТ-Восток', 'КВТ-Север'].includes(site.siteName)).map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: 🔥{site.gasConsumption || 0}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div data-svg-wrapper>
                      <svg width="2" height="40" viewBox="0 0 2 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M1 1L1 39" stroke="black" strokeOpacity="0.1" strokeLinecap="round"/>
                      </svg>
                    </div>

                    {/* Расход за предыдущий день */}
                    <div className="flex flex-col items-center gap-1">
                      <div className="justify-start text-black/50 text-xs font-normal font-open-sans leading-none">Расход за вчера</div>
                      <select 
                        className="text-xs border rounded px-2 py-1 w-48"
                        onChange={(e) => {
                          const selectedSite = metrics.previousDayConsumption?.find(site => site.siteName === e.target.value);
                          if (selectedSite) {
                            console.log('Выбран объект газа за вчера:', selectedSite);
                          }
                        }}
                      >
                        <option value="">Выберите объект</option>
                        {metrics.previousDayConsumption?.filter(site => ['КВТ-Юг', 'КВТ-Восток', 'КВТ-Север'].includes(site.siteName)).map((site, index) => (
                          <option key={index} value={site.siteName}>
                            {site.siteName}: 🔥{site.gasConsumption}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>
                </div>
              </div>

              {/* 3. User Menu Block */}
              <div className="bg-white/70 rounded-[20px] border border-neutral-400/30 px-4 lg:px-6 py-4 flex flex-col items-center justify-center">
                <div className="flex flex-col items-center gap-2 w-full">
                  <div className="w-36 h-11 px-4 py-1 rounded-[100px] outline outline-1 outline-offset-[-1px] outline-gray-200 inline-flex justify-start items-center gap-2.5 cursor-pointer" onClick={() => navigate('/settings')}>
                    <img src={settingsIcon} alt="settings" width="16" height="16" />
                    <div className="justify-start text-black text-xs font-semibold font-open-sans">Настройки</div>
                  </div>
                  
                  <div className="w-36 h-11 px-4 py-1 rounded-[100px] outline outline-1 outline-offset-[-1px] outline-gray-200 inline-flex justify-start items-center gap-2.5 cursor-pointer" onClick={() => navigate('/settings')}>
                    <img src={profileIcon} alt="profile" width="16" height="16" />
                    <div className="justify-start text-black text-xs font-semibold font-open-sans">{user?.name || 'Пользователь'}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Main Content Block */}
          {activeMenuItem === 1 && <DeviceAccounting />}
          {activeMenuItem === 2 && <DataVisualization />}
          {activeMenuItem === 3 && <EventLog />}
          {activeMenuItem === 4 && <UserActionsLog />}
          {activeMenuItem === 5 && <Reports />}
          {activeMenuItem === 6 && <AlarmLog />}
          {activeMenuItem === 7 && isAdmin && <Administration />}
        </main>
      </div>


    </div>
  );
}

export default Dashboard; 