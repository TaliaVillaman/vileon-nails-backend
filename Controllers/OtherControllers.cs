using Microsoft.AspNetCore.Mvc;
using VileonNails.Api.Models;
using VileonNails.Api.DTOs;
using VileonNails.Api.Services;

namespace VileonNails.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IVileonNailsStore _store;

        public ServicesController(IVileonNailsStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult GetServices() => Ok(_store.GetServices());

        [HttpPost]
        public IActionResult SaveService([FromBody] ServiceItem service)
        {
            var saved = _store.AddOrUpdateService(service);
            return Ok(saved);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ManicuristsController : ControllerBase
    {
        private readonly IVileonNailsStore _store;

        public ManicuristsController(IVileonNailsStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult GetManicurists() => Ok(_store.GetManicurists());
    }

    [ApiController]
    [Route("api/[controller]")]
    public class NailDesignsController : ControllerBase
    {
        private readonly IVileonNailsStore _store;

        public NailDesignsController(IVileonNailsStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult GetDesigns() => Ok(_store.GetNailDesigns());

        [HttpPost]
        public IActionResult AddDesign([FromBody] NailDesign design)
        {
            var newDesign = _store.AddNailDesign(design);
            return Ok(newDesign);
        }
    }

    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IVileonNailsStore _store;

        public WhatsAppController(IVileonNailsStore store)
        {
            _store = store;
        }

        [HttpGet("notifications")]
        public IActionResult GetNotifications() => Ok(_store.GetNotifications());

        [HttpPost("webhook/respond")]
        public IActionResult WebhookResponse([FromBody] WhatsAppWebhookDto dto)
        {
            string newStatus = dto.Action.ToUpper() switch
            {
                "CONFIRM" => "Confirmed",
                "CANCEL" => "Cancelled",
                "RESCHEDULE" => "Rescheduled",
                _ => "Pending"
            };

            var success = _store.UpdateAppointmentStatus(dto.AppointmentId, newStatus);
            if (!success) return NotFound(new { message = "Cita no encontrada." });

            var appt = _store.GetAppointmentById(dto.AppointmentId);
            string replyMessage = dto.Action.ToUpper() switch
            {
                "CONFIRM" => $"✅ ¡Excelente {appt?.ClientName}! Tu cita en Vileon Nails para el {appt?.AppointmentDate} a las {appt?.TimeSlot} ha sido CONFIRMADA.",
                "CANCEL" => $"❌ Entendido {appt?.ClientName}. Tu cita en Vileon Nails ha sido CANCELADA.",
                "RESCHEDULE" => $"🔄 Hola {appt?.ClientName}. Te hemos enviado el link para seleccionar nueva fecha y hora.",
                _ => "Mensaje recibido."
            };

            _store.LogNotification(dto.AppointmentId, appt?.ClientPhone ?? "", "WebhookReply", replyMessage);

            return Ok(new
            {
                success = true,
                newStatus,
                replyMessage,
                appointment = appt
            });
        }
    }
}
