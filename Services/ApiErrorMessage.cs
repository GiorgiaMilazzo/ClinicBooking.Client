using System.Net;
using System.Text.Json;

namespace ClinicBooking.Client.Services;

internal static class ApiErrorMessage
{
    public static async Task<string> ReadAsync(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "La sessione è scaduta. Accedi nuovamente.",
            HttpStatusCode.Forbidden => "Non hai i permessi per questa operazione (serve il ruolo admin).",
            _ when !string.IsNullOrWhiteSpace(message) => GetBodyMessage(message),
            _ => fallback
        };
    }

    private static string GetBodyMessage(string message)
    {
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errors))
            {
                var validationMessages = errors.EnumerateObject()
                    .SelectMany(error => error.Value.EnumerateArray())
                    .Select(error => error.GetString())
                    .Where(error => !string.IsNullOrWhiteSpace(error));
                var combined = string.Join(" ", validationMessages);
                if (!string.IsNullOrWhiteSpace(combined))
                {
                    return combined;
                }
            }

            if (root.TryGetProperty("detail", out var detail) &&
                !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                return detail.GetString()!;
            }

            if (root.TryGetProperty("title", out var title) &&
                !string.IsNullOrWhiteSpace(title.GetString()))
            {
                return title.GetString()!;
            }
        }
        catch (JsonException)
        {
            // Le regole di business dell'API restituiscono testo semplice.
        }

        return message.Trim('"');
    }
}
