using System.Net;
using System.Net.Http.Json;
using ClinicBooking.Client.Models;

namespace ClinicBooking.Client.Services;

public sealed class DoctorService(HttpClient httpClient)
{
    public async Task<ApiResult<List<Doctor>>> GetAllAsync()
    {
        try
        {
            using var response = await httpClient.GetAsync("api/Doctors");
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<List<Doctor>>(
                    false,
                    (int)response.StatusCode,
                    ErrorMessage: await ApiErrorMessage.ReadAsync(response, "Errore nel caricamento dei medici."));
            }

            var doctors = await response.Content.ReadFromJsonAsync<List<Doctor>>();
            return new ApiResult<List<Doctor>>(true, (int)response.StatusCode, doctors ?? []);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<List<Doctor>>(
                false,
                (int)HttpStatusCode.ServiceUnavailable,
                ErrorMessage: "Impossibile contattare il server. Verifica che l'API sia avviata.");
        }
    }
}
