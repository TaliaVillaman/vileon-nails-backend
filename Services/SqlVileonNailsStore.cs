using Microsoft.Data.SqlClient;
using Dapper;
using VileonNails.Api.Models;
using VileonNails.Api.DTOs;

namespace VileonNails.Api.Services
{
    public class SqlVileonNailsStore : IVileonNailsStore
    {
        private readonly string _connectionString;
        private readonly VileonNailsStore _fallbackStore;
        private readonly ILogger<SqlVileonNailsStore> _logger;

        public SqlVileonNailsStore(IConfiguration configuration, ILogger<SqlVileonNailsStore> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=VileonNailsDB;Trusted_Connection=True;TrustServerCertificate=True;";
            _logger = logger;
            _fallbackStore = new VileonNailsStore();
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public List<ServiceItem> GetServices()
        {
            try
            {
                using var conn = GetConnection();
                var sql = "SELECT Id, Name, Category, Description, PriceRD, DurationMinutes, ImageUrl, IsActive FROM dbo.Services WHERE IsActive = 1 ORDER BY Id";
                var result = conn.Query<ServiceItem>(sql).ToList();
                return result.Count > 0 ? result : _fallbackStore.GetServices();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Services from SQL Server. Using fallback data.");
                return _fallbackStore.GetServices();
            }
        }

        public ServiceItem? GetServiceById(int id)
        {
            try
            {
                using var conn = GetConnection();
                var sql = "SELECT Id, Name, Category, Description, PriceRD, DurationMinutes, ImageUrl, IsActive FROM dbo.Services WHERE Id = @Id";
                return conn.QueryFirstOrDefault<ServiceItem>(sql, new { Id = id }) ?? _fallbackStore.GetServiceById(id);
            }
            catch
            {
                return _fallbackStore.GetServiceById(id);
            }
        }

        public ServiceItem AddOrUpdateService(ServiceItem service)
        {
            try
            {
                using var conn = GetConnection();
                if (service.Id == 0)
                {
                    var sql = @"
                        INSERT INTO dbo.Services (Name, Category, Description, PriceRD, DurationMinutes, ImageUrl, IsActive)
                        VALUES (@Name, @Category, @Description, @PriceRD, @DurationMinutes, @ImageUrl, @IsActive);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
                    service.Id = conn.ExecuteScalar<int>(sql, service);
                }
                else
                {
                    var sql = @"
                        UPDATE dbo.Services 
                        SET Name = @Name, Category = @Category, Description = @Description, 
                            PriceRD = @PriceRD, DurationMinutes = @DurationMinutes, ImageUrl = @ImageUrl, IsActive = @IsActive
                        WHERE Id = @Id;";
                    conn.Execute(sql, service);
                }
                return service;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error saving service to SQL Server.");
                return _fallbackStore.AddOrUpdateService(service);
            }
        }

        public List<Manicurist> GetManicurists()
        {
            try
            {
                using var conn = GetConnection();
                var sql = "SELECT Id, FullName, Specialty, Rating, PhotoUrl, WorkingHours, IsAvailable FROM dbo.Manicurists WHERE IsAvailable = 1 ORDER BY Id";
                var result = conn.Query<Manicurist>(sql).ToList();
                return result.Count > 0 ? result : _fallbackStore.GetManicurists();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Manicurists from SQL Server.");
                return _fallbackStore.GetManicurists();
            }
        }

        public Manicurist? GetManicuristById(int id)
        {
            try
            {
                using var conn = GetConnection();
                var sql = "SELECT Id, FullName, Specialty, Rating, PhotoUrl, WorkingHours, IsAvailable FROM dbo.Manicurists WHERE Id = @Id";
                return conn.QueryFirstOrDefault<Manicurist>(sql, new { Id = id }) ?? _fallbackStore.GetManicuristById(id);
            }
            catch
            {
                return _fallbackStore.GetManicuristById(id);
            }
        }

        public List<NailDesign> GetNailDesigns()
        {
            try
            {
                using var conn = GetConnection();
                var sql = @"
                    SELECT d.Id, d.Title, d.Category, d.ImageUrl, d.LikesCount, d.ManicuristId, m.FullName AS ManicuristName
                    FROM dbo.NailDesigns d
                    INNER JOIN dbo.Manicurists m ON d.ManicuristId = m.Id
                    ORDER BY d.LikesCount DESC";
                var result = conn.Query<NailDesign>(sql).ToList();
                return result.Count > 0 ? result : _fallbackStore.GetNailDesigns();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching NailDesigns from SQL Server.");
                return _fallbackStore.GetNailDesigns();
            }
        }

        public NailDesign AddNailDesign(NailDesign design)
        {
            try
            {
                using var conn = GetConnection();
                var sql = @"
                    INSERT INTO dbo.NailDesigns (Title, Category, ImageUrl, LikesCount, ManicuristId)
                    VALUES (@Title, @Category, @ImageUrl, @LikesCount, @ManicuristId);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                design.Id = conn.ExecuteScalar<int>(sql, design);
                return design;
            }
            catch
            {
                return _fallbackStore.AddNailDesign(design);
            }
        }

        public List<Appointment> GetAppointments()
        {
            try
            {
                using var conn = GetConnection();
                var sql = @"
                    SELECT a.Id, a.BookingCode, a.ClientName, a.ClientPhone, a.ServiceId, s.Name AS ServiceName,
                           a.ManicuristId, m.FullName AS ManicuristName, CONVERT(VARCHAR(10), a.AppointmentDate, 120) AS AppointmentDate,
                           a.TimeSlot, a.Status, a.TotalAmountRD, a.Notes, a.CreatedAt
                    FROM dbo.Appointments a
                    LEFT JOIN dbo.Services s ON a.ServiceId = s.Id
                    LEFT JOIN dbo.Manicurists m ON a.ManicuristId = m.Id
                    ORDER BY a.Id DESC";
                var result = conn.Query<Appointment>(sql).ToList();
                return result.Count > 0 ? result : _fallbackStore.GetAppointments();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Appointments from SQL Server.");
                return _fallbackStore.GetAppointments();
            }
        }

        public Appointment? GetAppointmentByCode(string code)
        {
            try
            {
                using var conn = GetConnection();
                var sql = @"
                    SELECT a.Id, a.BookingCode, a.ClientName, a.ClientPhone, a.ServiceId, s.Name AS ServiceName,
                           a.ManicuristId, m.FullName AS ManicuristName, CONVERT(VARCHAR(10), a.AppointmentDate, 120) AS AppointmentDate,
                           a.TimeSlot, a.Status, a.TotalAmountRD, a.Notes, a.CreatedAt
                    FROM dbo.Appointments a
                    LEFT JOIN dbo.Services s ON a.ServiceId = s.Id
                    LEFT JOIN dbo.Manicurists m ON a.ManicuristId = m.Id
                    WHERE a.BookingCode = @Code OR a.ClientPhone = @Code";
                return conn.QueryFirstOrDefault<Appointment>(sql, new { Code = code }) ?? _fallbackStore.GetAppointmentByCode(code);
            }
            catch
            {
                return _fallbackStore.GetAppointmentByCode(code);
            }
        }

        public Appointment? GetAppointmentById(int id)
        {
            try
            {
                using var conn = GetConnection();
                var sql = @"
                    SELECT a.Id, a.BookingCode, a.ClientName, a.ClientPhone, a.ServiceId, s.Name AS ServiceName,
                           a.ManicuristId, m.FullName AS ManicuristName, CONVERT(VARCHAR(10), a.AppointmentDate, 120) AS AppointmentDate,
                           a.TimeSlot, a.Status, a.TotalAmountRD, a.Notes, a.CreatedAt
                    FROM dbo.Appointments a
                    LEFT JOIN dbo.Services s ON a.ServiceId = s.Id
                    LEFT JOIN dbo.Manicurists m ON a.ManicuristId = m.Id
                    WHERE a.Id = @Id";
                return conn.QueryFirstOrDefault<Appointment>(sql, new { Id = id }) ?? _fallbackStore.GetAppointmentById(id);
            }
            catch
            {
                return _fallbackStore.GetAppointmentById(id);
            }
        }

        public Appointment CreateAppointment(CreateAppointmentDto dto)
        {
            try
            {
                using var conn = GetConnection();
                var service = GetServiceById(dto.ServiceId);
                var manicurist = GetManicuristById(dto.ManicuristId);

                var bookingCode = $"VN-{Random.Shared.Next(1000, 9999)}";
                var totalAmount = service?.PriceRD ?? 1500m;

                var sql = @"
                    INSERT INTO dbo.Appointments (BookingCode, ClientName, ClientPhone, ServiceId, ManicuristId, AppointmentDate, TimeSlot, Status, TotalAmountRD, Notes)
                    VALUES (@BookingCode, @ClientName, @ClientPhone, @ServiceId, @ManicuristId, @AppointmentDate, @TimeSlot, 'Pending', @TotalAmountRD, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var newId = conn.ExecuteScalar<int>(sql, new
                {
                    BookingCode = bookingCode,
                    ClientName = dto.ClientName,
                    ClientPhone = dto.ClientPhone,
                    ServiceId = dto.ServiceId,
                    ManicuristId = dto.ManicuristId,
                    AppointmentDate = dto.AppointmentDate,
                    TimeSlot = dto.TimeSlot,
                    TotalAmountRD = totalAmount,
                    Notes = dto.Notes
                });

                var appt = new Appointment
                {
                    Id = newId,
                    BookingCode = bookingCode,
                    ClientName = dto.ClientName,
                    ClientPhone = dto.ClientPhone,
                    ServiceId = dto.ServiceId,
                    ServiceName = service?.Name ?? "Servicio de Uñas",
                    ManicuristId = dto.ManicuristId,
                    ManicuristName = manicurist?.FullName ?? "Manicurista Asignada",
                    AppointmentDate = dto.AppointmentDate,
                    TimeSlot = dto.TimeSlot,
                    Status = "Pending",
                    TotalAmountRD = totalAmount,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Log WhatsApp notification
                LogNotification(newId, dto.ClientPhone, "BookingConfirmation",
                    $"💅 Hola, {dto.ClientName}.\nTu cita en Vileon Nails ha sido registrada.\n\n📅 {dto.AppointmentDate}\n🕐 {dto.TimeSlot}\n💅 {appt.ServiceName}\n💰 RD$ {totalAmount:N0}\n\n¿Deseas confirmar tu cita?");

                return appt;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating appointment in SQL Server. Using fallback store.");
                return _fallbackStore.CreateAppointment(dto);
            }
        }

        public bool UpdateAppointmentStatus(int id, string status)
        {
            try
            {
                using var conn = GetConnection();
                var sql = "UPDATE dbo.Appointments SET Status = @Status WHERE Id = @Id";
                var rows = conn.Execute(sql, new { Status = status, Id = id });
                _fallbackStore.UpdateAppointmentStatus(id, status);
                return rows > 0;
            }
            catch
            {
                return _fallbackStore.UpdateAppointmentStatus(id, status);
            }
        }

        public bool RescheduleAppointment(int id, string newDate, string newTimeSlot)
        {
            try
            {
                using var conn = GetConnection();
                var sql = "UPDATE dbo.Appointments SET AppointmentDate = @NewDate, TimeSlot = @NewTimeSlot, Status = 'Rescheduled' WHERE Id = @Id";
                var rows = conn.Execute(sql, new { NewDate = newDate, NewTimeSlot = newTimeSlot, Id = id });
                _fallbackStore.RescheduleAppointment(id, newDate, newTimeSlot);
                return rows > 0;
            }
            catch
            {
                return _fallbackStore.RescheduleAppointment(id, newDate, newTimeSlot);
            }
        }

        public DashboardSummaryDto GetDashboardSummary()
        {
            try
            {
                using var conn = GetConnection();
                var sql = "SELECT * FROM dbo.vw_DashboardSummary";
                var summary = conn.QueryFirstOrDefault<DashboardSummaryDto>(sql);
                return summary ?? _fallbackStore.GetDashboardSummary();
            }
            catch
            {
                return _fallbackStore.GetDashboardSummary();
            }
        }

        public List<WhatsAppNotification> GetNotifications()
        {
            try
            {
                using var conn = GetConnection();
                var sql = "SELECT Id, AppointmentId, Phone, MessageType, Content, SentAt, Status FROM dbo.WhatsAppNotifications ORDER BY Id DESC";
                var result = conn.Query<WhatsAppNotification>(sql).ToList();
                return result.Count > 0 ? result : _fallbackStore.GetNotifications();
            }
            catch
            {
                return _fallbackStore.GetNotifications();
            }
        }

        public WhatsAppNotification LogNotification(int appointmentId, string phone, string messageType, string content)
        {
            try
            {
                using var conn = GetConnection();
                var sql = @"
                    INSERT INTO dbo.WhatsAppNotifications (AppointmentId, Phone, MessageType, Content, SentAt, Status)
                    VALUES (@AppointmentId, @Phone, @MessageType, @Content, GETUTCDATE(), 'Sent');
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                var newId = conn.ExecuteScalar<int>(sql, new { AppointmentId = appointmentId, Phone = phone, MessageType = messageType, Content = content });
                var notif = new WhatsAppNotification
                {
                    Id = newId,
                    AppointmentId = appointmentId,
                    Phone = phone,
                    MessageType = messageType,
                    Content = content,
                    SentAt = DateTime.UtcNow,
                    Status = "Sent"
                };
                return notif;
            }
            catch
            {
                return _fallbackStore.LogNotification(appointmentId, phone, messageType, content);
            }
        }
    }
}
