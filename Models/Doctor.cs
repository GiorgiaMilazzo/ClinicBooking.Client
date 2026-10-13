namespace ClinicBooking.Client.Models;

public sealed class Doctor
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Lastname { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public DateTime RegistrationDate { get; set; }
}
