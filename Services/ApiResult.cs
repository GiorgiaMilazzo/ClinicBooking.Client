namespace ClinicBooking.Client.Services;

public sealed record ApiResult<T>(
    bool Success,
    int StatusCode,
    T? Data = default,
    string? ErrorMessage = null);
