namespace VileonNails.Api.DTOs
{
    public class CreateAppointmentDto
    {
        public string ClientName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public int ManicuristId { get; set; }
        public string AppointmentDate { get; set; } = string.Empty; // YYYY-MM-DD
        public string TimeSlot { get; set; } = string.Empty;       // e.g. "03:00 PM"
        public string? Notes { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty; // Pending, Confirmed, Completed, Cancelled
    }

    public class RescheduleDto
    {
        public string NewDate { get; set; } = string.Empty;
        public string NewTimeSlot { get; set; } = string.Empty;
    }

    public class DashboardSummaryDto
    {
        public int TotalAppointmentsToday { get; set; }
        public int TotalAppointmentsTomorrow { get; set; }
        public decimal TotalRevenueRD { get; set; }
        public int ConfirmedCount { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class WhatsAppWebhookDto
    {
        public int AppointmentId { get; set; }
        public string Action { get; set; } = string.Empty; // "CONFIRM", "CANCEL", "RESCHEDULE"
    }
}
