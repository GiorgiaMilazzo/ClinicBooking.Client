namespace ClinicBooking.Client.Models;

public sealed class Patient
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Lastname { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
}
