using System.Net;
using System.Net.Http.Json;
using ClinicBooking.Client.Models;

namespace ClinicBooking.Client.Services;

public sealed class AppointmentService(HttpClient httpClient)
{
    public async Task<ApiResult<List<Appointment>>> GetAllAsync()
    {
        try
        {
            using var response = await httpClient.GetAsync("api/Appointments");
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<List<Appointment>>(
                    false,
                    (int)response.StatusCode,
                    ErrorMessage: await ApiErrorMessage.ReadAsync(
                        response,
                        "Si è verificato un errore durante il caricamento degli appuntamenti."));
            }

            var appointments = await response.Content.ReadFromJsonAsync<List<Appointment>>();
            return new ApiResult<List<Appointment>>(true, (int)response.StatusCode, appointments ?? []);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<List<Appointment>>(
                false,
                (int)HttpStatusCode.ServiceUnavailable,
                ErrorMessage: "Impossibile contattare il server. Verifica che l'API sia avviata.");
        }
    }

    public async Task<ApiResult<Appointment>> CreateAsync(CreateAppointmentRequest request)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("api/Appointments", request);
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<Appointment>(
                    false,
                    (int)response.StatusCode,
                    ErrorMessage: await ApiErrorMessage.ReadAsync(
                        response,
                        "Impossibile creare l'appuntamento."));
            }

            var appointment = await response.Content.ReadFromJsonAsync<Appointment>();
            return new ApiResult<Appointment>(true, (int)response.StatusCode, appointment);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<Appointment>(
                false,
                (int)HttpStatusCode.ServiceUnavailable,
                ErrorMessage: "Impossibile contattare il server. Verifica che l'API sia avviata.");
        }
    }
}
