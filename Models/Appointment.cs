namespace VileonNails.Api.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int ManicuristId { get; set; }
        public string ManicuristName { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty; // Format: YYYY-MM-DD
        public string TimeSlot { get; set; } = string.Empty;       // e.g. "03:00 PM"
        public string Status { get; set; } = "Pending";            // Pending, Confirmed, Completed, Cancelled, Rescheduled
        public decimal TotalAmountRD { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
