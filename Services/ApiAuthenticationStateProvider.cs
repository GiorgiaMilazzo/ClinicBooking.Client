using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClinicBooking.Client.Services;

public sealed class ApiAuthenticationStateProvider(ILocalStorageService localStorage)
    : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsStringAsync("authToken");
        var principal = CreatePrincipal(token);
        return new AuthenticationState(principal);
    }

    public void NotifyUserChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task<ClaimsPrincipal> CreatePrincipalAsync(string? token)
    {
        return CreatePrincipal(token);
    }

    private ClaimsPrincipal CreatePrincipal(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        try
        {
            var payload = token.Split('.');
            if (payload.Length < 2)
            {
                return Anonymous;
            }

            var json = Encoding.UTF8.GetString(Base64UrlDecode(payload[1]));
            using var document = JsonDocument.Parse(json);
            var claims = new List<Claim>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.NameEquals("exp"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Number &&
                        property.Value.TryGetInt64(out var expiry) &&
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiry)
                    {
                        _ = localStorage.RemoveItemAsync("authToken");
                        return Anonymous;
                    }

                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    claims.AddRange(property.Value.EnumerateArray()
                        .Select(value => new Claim(MapClaimType(property.Name), value.ToString())));
                }
                else
                {
                    claims.Add(new Claim(MapClaimType(property.Name), property.Value.ToString()));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "jwt",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);
            return new ClaimsPrincipal(identity);
        }
        catch (JsonException)
        {
            _ = localStorage.RemoveItemAsync("authToken");
            return Anonymous;
        }
        catch (FormatException)
        {
            _ = localStorage.RemoveItemAsync("authToken");
            return Anonymous;
        }
    }

    private static string MapClaimType(string claimType)
    {
        return claimType switch
        {
            "role" => ClaimTypes.Role,
            "unique_name" => ClaimTypes.Name,
            "name" => ClaimTypes.Name,
            "sub" => ClaimTypes.NameIdentifier,
            _ => claimType
        };
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
