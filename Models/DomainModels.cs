namespace VileonNails.Api.Models
{
    public class ServiceItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PriceRD { get; set; }
        public int DurationMinutes { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class Manicurist
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public double Rating { get; set; } = 5.0;
        public string PhotoUrl { get; set; } = string.Empty;
        public string WorkingHours { get; set; } = "9:00 AM - 7:00 PM";
        public bool IsAvailable { get; set; } = true;
    }

    public class NailDesign
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int ManicuristId { get; set; }
        public string ManicuristName { get; set; } = string.Empty;
    }

    public class WhatsAppNotification
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string MessageType { get; set; } = "BookingConfirmation"; // BookingConfirmation, Reminder, Cancellation
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Sent"; // Sent, Delivered, Read, Replied
    }
}
