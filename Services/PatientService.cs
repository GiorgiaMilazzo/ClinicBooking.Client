using System.Net;
using System.Net.Http.Json;
using ClinicBooking.Client.Models;

namespace ClinicBooking.Client.Services;

public sealed class PatientService(HttpClient httpClient)
{
    public async Task<ApiResult<List<Patient>>> GetAllAsync()
    {
        try
        {
            using var response = await httpClient.GetAsync("api/Patients");
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<List<Patient>>(
                    false,
                    (int)response.StatusCode,
                    ErrorMessage: await ApiErrorMessage.ReadAsync(response, "Errore nel caricamento dei pazienti."));
            }

            var patients = await response.Content.ReadFromJsonAsync<List<Patient>>();
            return new ApiResult<List<Patient>>(true, (int)response.StatusCode, patients ?? []);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<List<Patient>>(
                false,
                (int)HttpStatusCode.ServiceUnavailable,
                ErrorMessage: "Impossibile contattare il server. Verifica che l'API sia avviata.");
        }
    }
}
