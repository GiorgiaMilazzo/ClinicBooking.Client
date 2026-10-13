using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace ClinicBooking.Client.Services;

public sealed class AuthService(
    IHttpClientFactory httpClientFactory,
    ILocalStorageService localStorage,
    ApiAuthenticationStateProvider authenticationStateProvider)
{
    public async Task<string?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("Api")
                .PostAsJsonAsync("api/Login", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorAsync(response, "Email o password errati.");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (string.IsNullOrWhiteSpace(result?.Token))
            {
                return "La risposta del server non contiene un token valido.";
            }

            await localStorage.SetItemAsStringAsync("authToken", result.Token);
            authenticationStateProvider.NotifyUserChanged();
            return null;
        }
        catch (HttpRequestException)
        {
            return "Impossibile contattare il server. Verifica che l'API sia avviata.";
        }
        catch (InvalidOperationException)
        {
            return "La risposta del server non è valida.";
        }
    }

    public async Task<string?> RegisterAsync(string userName, string email, string password)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("Api")
                .PostAsJsonAsync("api/Register", new { userName, email, password });
            return response.IsSuccessStatusCode
                ? null
                : await ReadErrorAsync(response, "Registrazione non riuscita.");
        }
        catch (HttpRequestException)
        {
            return "Impossibile contattare il server. Verifica che l'API sia avviata.";
        }
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync("authToken");
        authenticationStateProvider.NotifyUserChanged();
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? fallback : message.Trim('"');
    }

    private sealed record LoginResponse(string Token);
}
