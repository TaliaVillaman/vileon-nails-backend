using System.Collections.Concurrent;
using VileonNails.Api.Models;
using VileonNails.Api.DTOs;

namespace VileonNails.Api.Services
{
    public interface IVileonNailsStore
    {
        List<ServiceItem> GetServices();
        ServiceItem? GetServiceById(int id);
        ServiceItem AddOrUpdateService(ServiceItem service);
        
        List<Manicurist> GetManicurists();
        Manicurist? GetManicuristById(int id);
        
        List<NailDesign> GetNailDesigns();
        NailDesign AddNailDesign(NailDesign design);
        
        List<Appointment> GetAppointments();
        Appointment? GetAppointmentByCode(string code);
        Appointment? GetAppointmentById(int id);
        Appointment CreateAppointment(CreateAppointmentDto dto);
        bool UpdateAppointmentStatus(int id, string status);
        bool RescheduleAppointment(int id, string newDate, string newTimeSlot);
        DashboardSummaryDto GetDashboardSummary();
        
        List<WhatsAppNotification> GetNotifications();
        WhatsAppNotification LogNotification(int appointmentId, string phone, string messageType, string content);
    }

    public class VileonNailsStore : IVileonNailsStore
    {
        private readonly List<ServiceItem> _services = new();
        private readonly List<Manicurist> _manicurists = new();
        private readonly List<NailDesign> _designs = new();
        private readonly ConcurrentBag<Appointment> _appointments = new();
        private readonly ConcurrentBag<WhatsAppNotification> _notifications = new();
        private int _appointmentIdCounter = 100;

        public VileonNailsStore()
        {
            SeedData();
        }

        private void SeedData()
        {
            // Seed Services in RD$
            _services.AddRange(new[]
            {
                new ServiceItem { Id = 1, Name = "Acrílico + Gel Polish", Category = "Acrílicas", Description = "Set completo de uñas acrílicas con acabado en gel de alta duración.", PriceRD = 1500m, DurationMinutes = 120, ImageUrl = "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=600&q=80", IsActive = true },
                new ServiceItem { Id = 2, Name = "Manicura Rusa Combinada", Category = "Manicura Rusa", Description = "Limpieza profunda de cutícula con torno y esmaltado impecable bajo cutícula.", PriceRD = 1200m, DurationMinutes = 90, ImageUrl = "https://images.unsplash.com/photo-1632345031435-8727f6897d53?auto=format&fit=crop&w=600&q=80", IsActive = true },
                new ServiceItem { Id = 3, Name = "Soft Gel Extensions (Tips)", Category = "Soft Gel", Description = "Extensión de uñas flexibles en gel premoldeado. Súper livianas y duraderas.", PriceRD = 1800m, DurationMinutes = 90, ImageUrl = "https://images.unsplash.com/photo-1519014816548-bf5fe059798b?auto=format&fit=crop&w=600&q=80", IsActive = true },
                new ServiceItem { Id = 4, Name = "Pedicura Spa & Gel", Category = "Pedicura Spa", Description = "Exfoliación profunda, baño de sales, hidratación y esmaltado permanente.", PriceRD = 1400m, DurationMinutes = 75, ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=600&q=80", IsActive = true },
                new ServiceItem { Id = 5, Name = "Nail Art 3D & Cristales (Set Completo)", Category = "Nail Art", Description = "Diseño artístico personalizado con pedrería Swarosvki y relieves 3D.", PriceRD = 2200m, DurationMinutes = 150, ImageUrl = "https://images.unsplash.com/photo-1599940824399-b87987ceb72a?auto=format&fit=crop&w=600&q=80", IsActive = true }
            });

            // Seed Manicurists
            _manicurists.AddRange(new[]
            {
                new Manicurist { Id = 1, FullName = "Valeria Morales", Specialty = "Especialista en Acrílico & 3D Art", Rating = 4.9, PhotoUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=300&q=80" },
                new Manicurist { Id = 2, FullName = "Carolina Gómez", Specialty = "Master en Manicura Rusa & Soft Gel", Rating = 5.0, PhotoUrl = "https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=300&q=80" },
                new Manicurist { Id = 3, FullName = "Ana Luisa Reyes", Specialty = "Pedicura Spa & Esmaltado Permanente", Rating = 4.8, PhotoUrl = "https://images.unsplash.com/photo-1567532939604-b6b5b0db2604?auto=format&fit=crop&w=300&q=80" }
            });

            // Seed Portfolio Designs
            _designs.AddRange(new[]
            {
                new NailDesign { Id = 1, Title = "Francés Moderno con Hoja de Oro", Category = "Acrílicas", ImageUrl = "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=600&q=80", LikesCount = 142, ManicuristId = 1, ManicuristName = "Valeria Morales" },
                new NailDesign { Id = 2, Title = "Nude Elegante Manicura Rusa", Category = "Manicura Rusa", ImageUrl = "https://images.unsplash.com/photo-1632345031435-8727f6897d53?auto=format&fit=crop&w=600&q=80", LikesCount = 98, ManicuristId = 2, ManicuristName = "Carolina Gómez" },
                new NailDesign { Id = 3, Title = "Glamour 3D con Cristales Swarosvki", Category = "Nail Art", ImageUrl = "https://images.unsplash.com/photo-1599940824399-b87987ceb72a?auto=format&fit=crop&w=600&q=80", LikesCount = 210, ManicuristId = 1, ManicuristName = "Valeria Morales" }
            });

            // Seed Initial Appointments for Demonstration
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            string tomorrowStr = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            _appointments.Add(new Appointment
            {
                Id = 1,
                BookingCode = "VN-1001",
                ClientName = "María Rodríguez",
                ClientPhone = "+18095550192",
                ServiceId = 1,
                ServiceName = "Acrílico + Gel Polish",
                ManicuristId = 1,
                ManicuristName = "Valeria Morales",
                AppointmentDate = todayStr,
                TimeSlot = "09:00 AM",
                Status = "Confirmed",
                TotalAmountRD = 1500m,
                Notes = "Preferencia por forma almendrada",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            });

            _appointments.Add(new Appointment
            {
                Id = 2,
                BookingCode = "VN-1002",
                ClientName = "Ana Martínez",
                ClientPhone = "+18095550188",
                ServiceId = 2,
                ServiceName = "Manicura Rusa Combinada",
                ManicuristId = 2,
                ManicuristName = "Carolina Gómez",
                AppointmentDate = todayStr,
                TimeSlot = "10:30 AM",
                Status = "Pending",
                TotalAmountRD = 1200m,
                Notes = "Primera vez en el estudio",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            });

            _appointments.Add(new Appointment
            {
                Id = 3,
                BookingCode = "VN-1003",
                ClientName = "Carolina Peña",
                ClientPhone = "+18095550177",
                ServiceId = 4,
                ServiceName = "Pedicura Spa & Gel",
                ManicuristId = 3,
                ManicuristName = "Ana Luisa Reyes",
                AppointmentDate = todayStr,
                TimeSlot = "12:00 PM",
                Status = "Completed",
                TotalAmountRD = 1400m,
                Notes = "Clienta VIP",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            });

            _appointments.Add(new Appointment
            {
                Id = 4,
                BookingCode = "VN-1004",
                ClientName = "Laura Torres",
                ClientPhone = "+18095550144",
                ServiceId = 3,
                ServiceName = "Soft Gel Extensions",
                ManicuristId = 2,
                ManicuristName = "Carolina Gómez",
                AppointmentDate = tomorrowStr,
                TimeSlot = "02:00 PM",
                Status = "Confirmed",
                TotalAmountRD = 1800m,
                Notes = "",
                CreatedAt = DateTime.UtcNow
            });
        }

        public List<ServiceItem> GetServices() => _services.Where(s => s.IsActive).ToList();
        public ServiceItem? GetServiceById(int id) => _services.FirstOrDefault(s => s.Id == id);
        
        public ServiceItem AddOrUpdateService(ServiceItem service)
        {
            if (service.Id == 0)
            {
                service.Id = _services.Max(s => s.Id) + 1;
                _services.Add(service);
            }
            else
            {
                var idx = _services.FindIndex(s => s.Id == service.Id);
                if (idx >= 0) _services[idx] = service;
            }
            return service;
        }

        public List<Manicurist> GetManicurists() => _manicurists;
        public Manicurist? GetManicuristById(int id) => _manicurists.FirstOrDefault(m => m.Id == id);

        public List<NailDesign> GetNailDesigns() => _designs;
        public NailDesign AddNailDesign(NailDesign design)
        {
            design.Id = _designs.Count > 0 ? _designs.Max(d => d.Id) + 1 : 1;
            _designs.Insert(0, design);
            return design;
        }

        public List<Appointment> GetAppointments() => _appointments.OrderByDescending(a => a.CreatedAt).ToList();
        
        public Appointment? GetAppointmentByCode(string code)
        {
            return _appointments.FirstOrDefault(a => a.BookingCode.Equals(code, StringComparison.OrdinalIgnoreCase) || a.ClientPhone.Contains(code));
        }

        public Appointment? GetAppointmentById(int id) => _appointments.FirstOrDefault(a => a.Id == id);

        public Appointment CreateAppointment(CreateAppointmentDto dto)
        {
            var service = GetServiceById(dto.ServiceId);
            var manicurist = GetManicuristById(dto.ManicuristId);

            int newId = Interlocked.Increment(ref _appointmentIdCounter);
            var appointment = new Appointment
            {
                Id = newId,
                BookingCode = $"VN-{newId:D4}",
                ClientName = dto.ClientName,
                ClientPhone = dto.ClientPhone,
                ServiceId = dto.ServiceId,
                ServiceName = service?.Name ?? "Servicio de Uñas",
                ManicuristId = dto.ManicuristId,
                ManicuristName = manicurist?.FullName ?? "Manicurista Asignada",
                AppointmentDate = dto.AppointmentDate,
                TimeSlot = dto.TimeSlot,
                Status = "Pending",
                TotalAmountRD = service?.PriceRD ?? 1500m,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _appointments.Add(appointment);

            // Log automatic WhatsApp confirmation template message
            LogNotification(appointment.Id, appointment.ClientPhone, "BookingConfirmation",
                $"💅 Hola {appointment.ClientName}. Tu cita en Vileon Nails ha sido registrada.\n📅 {appointment.AppointmentDate} a las {appointment.TimeSlot}\n💅 {appointment.ServiceName}\n💰 RD$ {appointment.TotalAmountRD:N0}");

            return appointment;
        }

        public bool UpdateAppointmentStatus(int id, string status)
        {
            var appt = GetAppointmentById(id);
            if (appt == null) return false;
            appt.Status = status;
            return true;
        }

        public bool RescheduleAppointment(int id, string newDate, string newTimeSlot)
        {
            var appt = GetAppointmentById(id);
            if (appt == null) return false;
            appt.AppointmentDate = newDate;
            appt.TimeSlot = newTimeSlot;
            appt.Status = "Rescheduled";
            return true;
        }

        public DashboardSummaryDto GetDashboardSummary()
        {
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            string tomorrowStr = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            var todayList = _appointments.Where(a => a.AppointmentDate == todayStr).ToList();
            var tomorrowList = _appointments.Where(a => a.AppointmentDate == tomorrowStr).ToList();

            return new DashboardSummaryDto
            {
                TotalAppointmentsToday = todayList.Count,
                TotalAppointmentsTomorrow = tomorrowList.Count,
                TotalRevenueRD = todayList.Where(a => a.Status != "Cancelled").Sum(a => a.TotalAmountRD),
                ConfirmedCount = _appointments.Count(a => a.Status == "Confirmed"),
                PendingCount = _appointments.Count(a => a.Status == "Pending"),
                CompletedCount = _appointments.Count(a => a.Status == "Completed"),
                CancelledCount = _appointments.Count(a => a.Status == "Cancelled")
            };
        }

        public List<WhatsAppNotification> GetNotifications() => _notifications.OrderByDescending(n => n.SentAt).ToList();

        public WhatsAppNotification LogNotification(int appointmentId, string phone, string messageType, string content)
        {
            var notification = new WhatsAppNotification
            {
                Id = _notifications.Count + 1,
                AppointmentId = appointmentId,
                Phone = phone,
                MessageType = messageType,
                Content = content,
                SentAt = DateTime.UtcNow,
                Status = "Sent"
            };
            _notifications.Add(notification);
            return notification;
        }
    }
}
