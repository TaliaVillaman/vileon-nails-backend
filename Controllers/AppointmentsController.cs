using Microsoft.AspNetCore.Mvc;
using VileonNails.Api.Models;
using VileonNails.Api.DTOs;
using VileonNails.Api.Services;

namespace VileonNails.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IVileonNailsStore _store;

        public AppointmentsController(IVileonNailsStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult GetAppointments()
        {
            return Ok(_store.GetAppointments());
        }

        [HttpGet("summary")]
        public IActionResult GetDashboardSummary()
        {
            return Ok(_store.GetDashboardSummary());
        }

        [HttpGet("search/{codeOrPhone}")]
        public IActionResult SearchAppointment(string codeOrPhone)
        {
            var appointment = _store.GetAppointmentByCode(codeOrPhone);
            if (appointment == null) return NotFound(new { message = "No se encontró ninguna cita con el código o teléfono ingresado." });
            return Ok(appointment);
        }

        [HttpPost]
        public IActionResult CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ClientName) || string.IsNullOrWhiteSpace(dto.ClientPhone))
            {
                return BadRequest(new { message = "El nombre y teléfono de WhatsApp son requeridos." });
            }

            var appointment = _store.CreateAppointment(dto);
            return CreatedAtAction(nameof(SearchAppointment), new { codeOrPhone = appointment.BookingCode }, appointment);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var success = _store.UpdateAppointmentStatus(id, dto.Status);
            if (!success) return NotFound(new { message = "Cita no encontrada." });
            return Ok(new { message = $"Cita #{id} actualizada a estado: {dto.Status}" });
        }

        [HttpPost("{id}/reschedule")]
        public IActionResult Reschedule(int id, [FromBody] RescheduleDto dto)
        {
            var success = _store.RescheduleAppointment(id, dto.NewDate, dto.NewTimeSlot);
            if (!success) return NotFound(new { message = "Cita no encontrada." });
            return Ok(new { message = $"Cita #{id} reprogramada para {dto.NewDate} a las {dto.NewTimeSlot}" });
        }
    }
}
