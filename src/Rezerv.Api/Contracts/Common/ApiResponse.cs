namespace Rezerv.Api.Contracts.Common;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    IReadOnlyList<string>? Errors)
{
    public static ApiResponse<T> Succeeded(T data, string message) =>
        new(true, message, data, null);

    public static ApiResponse<T> Failed(string message, params string[] errors) =>
        new(false, message, default, errors.Length == 0 ? null : errors);
}