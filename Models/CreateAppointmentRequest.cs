using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Client.Models;

public sealed class CreateAppointmentRequest
{
    public DateTime DateTimeAppointment { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleziona un medico.")]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleziona un paziente.")]
    public int PatientId { get; set; }
}
