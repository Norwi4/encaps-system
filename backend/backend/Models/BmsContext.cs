using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class BmsContext : DbContext
{
    public BmsContext()
    {
    }

    public BmsContext(DbContextOptions<BmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceDatum> DeviceData { get; set; }

    public virtual DbSet<DeviceSetting> DeviceSettings { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<ElectricityDeviceDatum> ElectricityDeviceData { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Event1> Events1 { get; set; }

    public virtual DbSet<GasDeviceDatum> GasDeviceData { get; set; }

    public virtual DbSet<GasDeviceReference> GasDeviceReferences { get; set; }

    public virtual DbSet<Object> Objects { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<Protocol> Protocols { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAction> UserActions { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserRole1> UserRoles1 { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<VendorModel> VendorModels { get; set; }

    public virtual DbSet<ConsumptionByDay> ConsumptionByDay { get; set; }

    public virtual DbSet<ConsumptionByMonth> ConsumptionByMonth { get; set; }

    public virtual DbSet<ConsumptionByToday> ConsumptionByToday { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("action_pkey");

            entity.ToTable("action");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("channels_pkey");

            entity.ToTable("channels", tb => tb.HasComment("Каналы связи"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BaudRate)
                .HasDefaultValue(9600)
                .HasColumnName("baud_rate");
            entity.Property(e => e.CloseAfterSession)
                .HasDefaultValue(false)
                .HasColumnName("close_after_session");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.DataBits)
                .HasDefaultValue(8)
                .HasColumnName("data_bits");
            entity.Property(e => e.Dev)
                .HasComment("Канал, через который ведется связь")
                .HasColumnName("dev");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.Name)
                .HasComment("Наименование канала")
                .HasColumnName("name");
            entity.Property(e => e.OpenAttempts)
                .HasDefaultValue(1)
                .HasComment("Количество попыток соединения")
                .HasColumnName("open_attempts");
            entity.Property(e => e.OpenTimeout)
                .HasDefaultValue(1000)
                .HasColumnName("open_timeout");
            entity.Property(e => e.OverTcp)
                .HasDefaultValue(false)
                .HasComment("Данные поверх TCP соединения без изменений")
                .HasColumnName("over_tcp");
            entity.Property(e => e.Parity)
                .HasDefaultValueSql("'N'::\"char\"")
                .HasColumnType("char")
                .HasColumnName("parity");
            entity.Property(e => e.Port).HasColumnName("port");
            entity.Property(e => e.StopBits)
                .HasDefaultValue(1)
                .HasColumnName("stop_bits");
            entity.Property(e => e.TimeDisconnect)
                .HasComment("Время отключения при бездействии (для удерживаемых соединений)")
                .HasColumnName("time_disconnect");
            entity.Property(e => e.TypeLink)
                .HasDefaultValue(1)
                .HasColumnName("type_link");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("obj_device_pkey");

            entity.ToTable("device", tb => tb.HasComment("Устройства с привязкой к объекту"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.RequireRefresh)
                .HasDefaultValue(false)
                .HasComment("Требуется обновление настроек устройства")
                .HasColumnName("require_refresh");
            entity.Property(e => e.ChannelId)
                .HasDefaultValue(0L)
                .HasComment("ID канала передачи данных (channel)")
                .HasColumnName("channel_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.Image)
                .HasComment("Путь и название файла изображения")
                .HasColumnName("image");
            entity.Property(e => e.InstallationDate).HasColumnName("installation_date");
            entity.Property(e => e.LastReceive)
                .HasComment("Последний опрос")
                .HasColumnName("last_receive");
            entity.Property(e => e.Model)
                .HasComment("Модель")
                .HasColumnName("model");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.SerialNo)
                .HasComment("Серийный номер")
                .HasColumnName("serial_no");
            entity.Property(e => e.TrustedBefore)
                .HasComment("Дата действительности поверки")
                .HasColumnName("trusted_before");
            entity.Property(e => e.Vendor)
                .HasComment("Производитель")
                .HasColumnName("vendor");
            entity.Property(e => e.DeviceTypeId)
                .HasComment("ID типа устройства (electrical, gas, water)")
                .HasColumnName("device_type_id");
            entity.Property(e => e.SortId)
                .HasComment("ID для сортировки устройств на дашборде")
                .HasColumnName("sort_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.Devices)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("parent_id");
            
            entity.HasOne(d => d.DeviceType).WithMany()
                .HasForeignKey(d => d.DeviceTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("device_type_id_fkey");
            
            entity.HasOne(d => d.Channel).WithMany()
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("channel_id");
            
            entity.HasOne(d => d.VendorModel).WithMany()
                .HasForeignKey(d => d.Vendor)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("device_vendor_model_fkey");
        });

        modelBuilder.Entity<DeviceDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_data_pkey");

            entity.ToTable("device_data", tb => tb.HasComment("Данные, полученные с приборов"));

            entity.HasIndex(e => e.DeviceId, "device_data_device_id_idx").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Dt)
                .HasDefaultValueSql("now()")
                .HasComment("Дата / время записи/изменений")
                .HasColumnName("dt");
            entity.Property(e => e.JsonData)
                .HasColumnType("jsonb")
                .HasColumnName("json_data");
            entity.Property(e => e.Type)
                .HasComment("Тип полученной записи (накопленные данные, мощность и т.д.) - коды из справочника")
                .HasColumnName("type");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceData)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("device_data_device_id_fkey");
        });

        modelBuilder.Entity<DeviceSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_setting_pkey");

            entity.ToTable("device_setting", tb => tb.HasComment("Настройки связи для устройства"));

            entity.HasIndex(e => e.DeviceId, "device_setting_device_id_idx").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AuthLogin).HasColumnName("auth_login");
            entity.Property(e => e.AuthPass).HasColumnName("auth_pass");
            entity.Property(e => e.BadReceive)
                .HasDefaultValue(0L)
                .HasComment("Количество сбойных опросов")
                .HasColumnName("bad_receive");
            entity.Property(e => e.DataAddr)
                .HasDefaultValue(0L)
                .HasComment("Адрес начала данных в устройстве")
                .HasColumnName("data_addr");
            entity.Property(e => e.DataBit).HasColumnName("data_bit");
            entity.Property(e => e.DataSize)
                .HasDefaultValue(0)
                .HasComment("Размер данных")
                .HasColumnName("data_size");
            entity.Property(e => e.DayDataLive)
                .HasDefaultValue(90)
                .HasComment("Время хранения данных, дней")
                .HasColumnName("day_data_live");
            entity.Property(e => e.DevAddr)
                .HasDefaultValue(0)
                .HasComment("Адрес устройства")
                .HasColumnName("dev_addr");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.KoeffTrans)
                .HasComment("Коэффициент трансформации")
                .HasColumnName("koeff_trans");
            entity.Property(e => e.LastSession)
                .HasComment("Последний опрос")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_session");
            entity.Property(e => e.Parity)
                .HasColumnType("char")
                .HasColumnName("parity");
            entity.Property(e => e.ProtPostHandlerName)
                .HasComment("Постобработчик принятых данных. Применяется для финальной обработки принятых данных. Как правило со встроенным протоколом modbus")
                .HasColumnName("prot_post_handler_name");
            entity.Property(e => e.ProtServiceCode)
                .HasDefaultValue((short)0)
                .HasComment("Сервисный код для обработчика протоколов. Может использоваться для различных целей обработчиком, здесь можно указать, например, тип опрашиваемых этим устройством данных")
                .HasColumnName("prot_service_code");
            entity.Property(e => e.ProtocolId)
                .HasComment("Протокол опроса, ID")
                .HasColumnName("protocol_id");
            entity.Property(e => e.ScanInterval)
                .HasComment("Интервал опроса")
                .HasColumnName("scan_interval");
            entity.Property(e => e.ScanRepeat)
                .HasDefaultValue((short)2)
                .HasComment("Количество повторов сканирования при ошибке опроса")
                .HasColumnName("scan_repeat");
            entity.Property(e => e.ScanRepeatInterval)
                .HasDefaultValue((short)30)
                .HasComment("Интервал между повторными опросами, сек")
                .HasColumnName("scan_repeat_interval");
            entity.Property(e => e.Speed)
                .HasComment("Скорость связи")
                .HasColumnName("speed");
            entity.Property(e => e.StatusLastSession)
                .HasComment("Статус последнего опроса")
                .HasColumnName("status_last_session");
            entity.Property(e => e.StopBit).HasColumnName("stop_bit");
            entity.Property(e => e.SuccessReceive)
                .HasDefaultValue(0L)
                .HasComment("Количество успешных опросов")
                .HasColumnName("success_receive");
            entity.Property(e => e.TimeOut)
                .HasDefaultValue(200)
                .HasComment("Тайм аут ответа устройства")
                .HasColumnName("time_out");
            entity.Property(e => e.TypeLink)
                .HasComment("Тип линии связи\n1 - serial\n2 - TCP\n3 - UDP\n4 - modem")
                .HasColumnName("type_link");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceSettings)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("device_setting_device_id_fkey");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("device_type_pkey");

            entity.ToTable("device_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<ElectricityDeviceDatum>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("electricity_device_data");

            entity.Property(e => e.AllEnergy)
                .HasComment("Active energy SUMM")
                .HasColumnName("all_energy");
            entity.Property(e => e.AllEnergyK)
                .HasDefaultValueSql("0")
                .HasComment("Накопленная энергия с учетом коэффициента трансформации")
                .HasColumnName("all_energy_k");
            entity.Property(e => e.Angle1)
                .HasDefaultValueSql("0")
                .HasComment("Угол между фазными напряжениями")
                .HasColumnName("angle_1");
            entity.Property(e => e.Angle2)
                .HasDefaultValueSql("0")
                .HasComment("Угол между фазными напряжениями")
                .HasColumnName("angle_2");
            entity.Property(e => e.Angle3)
                .HasDefaultValueSql("0")
                .HasComment("Угол между фазными напряжениями")
                .HasColumnName("angle_3");
            entity.Property(e => e.Aq1)
                .HasDefaultValueSql("0")
                .HasComment("Apparent power L1")
                .HasColumnName("aq_1");
            entity.Property(e => e.Aq2)
                .HasDefaultValueSql("0")
                .HasComment("Apparent power L2")
                .HasColumnName("aq_2");
            entity.Property(e => e.Aq3)
                .HasDefaultValueSql("0")
                .HasComment("Apparent power L3")
                .HasColumnName("aq_3");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Freq).HasColumnName("freq");
            entity.Property(e => e.FundPfCf1)
                .HasDefaultValueSql("0")
                .HasComment("Fund power factor, CosPhi")
                .HasColumnName("fund_pf_cf1");
            entity.Property(e => e.FundPfCf2)
                .HasDefaultValueSql("0")
                .HasComment("Fund power factor, CosPhi")
                .HasColumnName("fund_pf_cf2");
            entity.Property(e => e.FundPfCf3)
                .HasDefaultValueSql("0")
                .HasComment("Fund power factor, CosPhi")
                .HasColumnName("fund_pf_cf3");
            entity.Property(e => e.HIL1)
                .HasDefaultValueSql("0")
                .HasComment("Harmonic THD I")
                .HasColumnName("h_i_l1");
            entity.Property(e => e.HIL2)
                .HasDefaultValueSql("0")
                .HasComment("Harmonic THD I")
                .HasColumnName("h_i_l2");
            entity.Property(e => e.HIL3)
                .HasDefaultValueSql("0")
                .HasComment("Harmonic THD I")
                .HasColumnName("h_i_l3");
            entity.Property(e => e.HUL1)
                .HasDefaultValueSql("0")
                .HasComment("Harmonic THD U")
                .HasColumnName("h_u_l1");
            entity.Property(e => e.HUL2)
                .HasDefaultValueSql("0")
                .HasComment("Harmonic THD U")
                .HasColumnName("h_u_l2");
            entity.Property(e => e.HUL3)
                .HasDefaultValueSql("0")
                .HasComment("Harmonic THD U")
                .HasColumnName("h_u_l3");
            entity.Property(e => e.IL1)
                .HasComment("Ток по фазе 1")
                .HasColumnName("i_l1");
            entity.Property(e => e.IL2).HasColumnName("i_l2");
            entity.Property(e => e.IL3).HasColumnName("i_l3");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.PL1)
                .HasComment("Активная мощность по фазе 1")
                .HasColumnName("p_l1");
            entity.Property(e => e.PL2).HasColumnName("p_l2");
            entity.Property(e => e.PL3).HasColumnName("p_l3");
            entity.Property(e => e.PSum).HasColumnName("p_sum");
            entity.Property(e => e.QL1)
                .HasComment("Реактивная мощность L1")
                .HasColumnName("q_L1");
            entity.Property(e => e.QL2).HasColumnName("q_l2");
            entity.Property(e => e.QL3).HasColumnName("q_l3");
            entity.Property(e => e.QSum).HasColumnName("q_sum");
            entity.Property(e => e.ReactQCL1)
                .HasDefaultValueSql("0")
                .HasComment("Reaktive energy capacitive")
                .HasColumnName("react_q_c_l1");
            entity.Property(e => e.ReactQCL2)
                .HasDefaultValueSql("0")
                .HasComment("Reaktive energy capacitive")
                .HasColumnName("react_q_c_l2");
            entity.Property(e => e.ReactQCL3)
                .HasDefaultValueSql("0")
                .HasComment("Reaktive energy capacitive")
                .HasColumnName("react_q_c_l3");
            entity.Property(e => e.ReactQIL1)
                .HasDefaultValueSql("0")
                .HasComment("Reaktive energy inductive")
                .HasColumnName("react_q_i_l1");
            entity.Property(e => e.ReactQIL2)
                .HasDefaultValueSql("0")
                .HasComment("Reaktive energy inductive")
                .HasColumnName("react_q_i_l2");
            entity.Property(e => e.ReactQIL3)
                .HasDefaultValueSql("0")
                .HasComment("Reaktive energy inductive")
                .HasColumnName("react_q_i_l3");
            entity.Property(e => e.ReactiveEnergySum)
                .HasComment("Reactive energy SUMM")
                .HasColumnName("reactive_energy_sum");
            entity.Property(e => e.RotationField)
                .HasDefaultValueSql("0")
                .HasComment("Rotation field")
                .HasColumnName("rotation_field");
            entity.Property(e => e.RqcL1)
                .HasDefaultValueSql("0")
                .HasComment("Real energy consumed")
                .HasColumnName("rqc_l1");
            entity.Property(e => e.RqcL2)
                .HasDefaultValueSql("0")
                .HasComment("Real energy consumed")
                .HasColumnName("rqc_l2");
            entity.Property(e => e.RqcL3)
                .HasDefaultValueSql("0")
                .HasComment("Real energy consumed")
                .HasColumnName("rqc_l3");
            entity.Property(e => e.RqdL1)
                .HasDefaultValueSql("0")
                .HasComment("Real energy delivered")
                .HasColumnName("rqd_l1");
            entity.Property(e => e.RqdL2)
                .HasDefaultValueSql("0")
                .HasComment("Real energy delivered")
                .HasColumnName("rqd_l2");
            entity.Property(e => e.RqdL3)
                .HasDefaultValueSql("0")
                .HasComment("Real energy delivered")
                .HasColumnName("rqd_l3");
            entity.Property(e => e.TimeReading)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time_reading");
            entity.Property(e => e.UL1L2).HasColumnName("u_l1_l2");
            entity.Property(e => e.UL1N)
                .HasComment("Напряжение l1_n")
                .HasColumnName("u_l1_n");
            entity.Property(e => e.UL2L3).HasColumnName("u_l2_l3");
            entity.Property(e => e.UL2N).HasColumnName("u_l2_n");
            entity.Property(e => e.UL3L1).HasColumnName("u_l3_l1");
            entity.Property(e => e.UL3N).HasColumnName("u_l3_n");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_pkey");

            entity.ToTable("events");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.ObjectId).HasColumnName("object_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.EventNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("events_event_id_fkey");
        });

        modelBuilder.Entity<Event1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("event_pkey");

            entity.ToTable("event");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.IsCritical).HasColumnName("is_critical");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<GasDeviceDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gas_device_data_pkey");

            entity.ToTable("gas_device_data", tb => tb.HasComment("Данные с газовых устройств"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BatteryLive)
                .HasDefaultValueSql("0")
                .HasComment("Осталось времени до исчерпания заряда батареи")
                .HasColumnName("battery_live");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.InstantaneousFlow).HasColumnName("instantaneous_flow");
            entity.Property(e => e.Power)
                .HasDefaultValueSql("0")
                .HasComment("Мощность")
                .HasColumnName("power");
            entity.Property(e => e.PressureGas)
                .HasDefaultValueSql("0")
                .HasComment("Давление газа на входе в счетчик")
                .HasColumnName("pressure_gas");
            entity.Property(e => e.ReadingTime)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time_reading");
            entity.Property(e => e.StandardVolume).HasColumnName("standard_volume");
            entity.Property(e => e.TemperatureGas).HasColumnName("temperature_gas");
            entity.Property(e => e.WorkingVolume).HasColumnName("working_volume");

            entity.HasOne(d => d.Device).WithMany(p => p.GasDeviceData)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gas_device_data_device_id_fkey");
        });

        modelBuilder.Entity<GasDeviceReference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gas_device_references_pkey");

            entity.ToTable("gas_device_references");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorrectionFactor).HasColumnName("correction_factor");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.SubstitutionCompressibility).HasColumnName("substitution_compressibility");
            entity.Property(e => e.SubstitutionPressure).HasColumnName("substitution_pressure");

            entity.HasOne(d => d.Device).WithMany(p => p.GasDeviceReferences)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gas_device_references_device_id_fkey");
        });

        modelBuilder.Entity<Object>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objects_pkey");

            entity.ToTable("objects", tb => tb.HasComment("Объекты, на которых установлены счетчики"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasComment("Комментарий, произвольный текст")
                .HasColumnName("comment");
            entity.Property(e => e.Name)
                .HasComment("Название объекта")
                .HasColumnName("name");
            entity.Property(e => e.Place)
                .HasComment("Местоположение")
                .HasColumnName("place");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("profile_pkey");

            entity.ToTable("profile");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FName).HasColumnName("f_name");
            entity.Property(e => e.LName).HasColumnName("l_name");
            entity.Property(e => e.Mail).HasColumnName("mail");
            entity.Property(e => e.Patronimyc).HasColumnName("patronimyc");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Protocol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("protocol_pkey");

            entity.ToTable("protocol", tb => tb.HasComment("Протоколы связи"));

            entity.HasIndex(e => e.CodeName, "protocol_code_name_idx").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CodeName)
                .HasComment("Код протокола. По нему осуществляется идентификация протокола")
                .HasColumnName("code_name");
            entity.Property(e => e.Name)
                .HasComment("Название протокола.")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("report_pkey");

            entity.ToTable("report");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Reports)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("fk_report_created_by");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles", tb => tb.HasComment("Справочник - роли доступа "));

            entity.HasIndex(e => e.RoleCode, "roles_role_code_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.RoleCode)
                .HasComment("Код роли, по нему связываются подчиненные таблицы")
                .HasColumnName("role_code");
            entity.Property(e => e.RoleName)
                .HasComment("Роль, описание")
                .HasColumnType("character varying(255)[]")
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("users", tb => tb.HasComment("Пользователи и права"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasColumnType("character varying")
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasComment("ФИО")
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasColumnType("character varying")
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasColumnType("character varying")
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .HasColumnType("character varying")
                .HasColumnName("surname");
        });

        modelBuilder.Entity<UserAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_actions /* Таблица действий польз_pkey");

            entity.ToTable("user_actions", tb => tb.HasComment("Таблица действий пользователей"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserActions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_actions /* Таблица действий п_user_id_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_role_pkey");

            entity.ToTable("user_role", tb => tb.HasComment("Роли пользователя"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AccessLevel)
                .HasComment("Уровень доступа к роли (закрыто, чтение, запись/чтение)")
                .HasColumnType("char")
                .HasColumnName("access_level");
            entity.Property(e => e.Role)
                .HasComment("Роль пользователя из справочника")
                .HasColumnName("role");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_role_user_id_fkey");
        });

        modelBuilder.Entity<UserRole1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_roles_pkey");

            entity.ToTable("user_roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RolesId).HasColumnName("roles_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vendors_pkey");

            entity.ToTable("vendors", tb => tb.HasComment("Таблица производителей"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<VendorModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vendor_models_pk");

            entity.ToTable("vendor_models", tb => tb.HasComment("Модели устройств с привязкой в вендору"));

            entity.Property(e => e.Id).HasColumnName("_id");
            entity.Property(e => e.Comment)
                .HasComment("Комментарий")
                .HasColumnType("character varying")
                .HasColumnName("comment");
            entity.Property(e => e.Name)
                .HasComment("Наименование модели")
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.PlateInfo)
                .HasComment("JSON с описанием выводимых полей для конкретного устройства")
                .HasColumnType("json")
                .HasColumnName("plate_info");

            entity.HasOne(d => d.Vendor).WithMany(p => p.VendorModels)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("vendor_models_vendors_id_fk");
        });

        modelBuilder.Entity<ConsumptionByDay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consumption_by_day_pkey");
            entity.ToTable("consumption_by_day");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dt).HasColumnName("dt");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
        });

        modelBuilder.Entity<ConsumptionByMonth>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consumption_by_month_pkey");
            entity.ToTable("consumption_by_month");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dt).HasColumnName("dt");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
        });

        modelBuilder.Entity<ConsumptionByToday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consumption_by_today_pkey");
            entity.ToTable("consumption_by_today");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dt).HasColumnName("dt");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
