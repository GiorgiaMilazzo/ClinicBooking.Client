namespace ClinicBooking.Client.Models;

public sealed class Appointment
{
    public int Id { get; set; }

    public DateTime DateTimeAppointment { get; set; }

    public AppointmentStatus Status { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;
}

public enum AppointmentStatus
{
    Booked = 0,
    Confirmed = 1,
    Complete = 2
}
