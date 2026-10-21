using System.Text.Json.Serialization;

namespace KiraathaneTactics.Application.Common.Models;

/// <summary>
/// Tek tip API yanıt zarfı. TÜM uçlar (başarı + hata) aynı şekli döner ki
/// web/iOS/Android istemciler tek bir generic model ile yanıtı işleyebilsin.
/// </summary>
/// <typeparam name="T">Başarı durumunda dönen veri tipi.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>İşlem başarılı mı? İstemci ilk olarak buna bakar.</summary>
    [JsonPropertyOrder(1)]
    public bool Success { get; init; }

    /// <summary>HTTP durum kodu (200, 201, 400, 404 ...). Gövdede de tekrarlanır ki istemci tek yerden okusun.</summary>
    [JsonPropertyOrder(2)]
    public int StatusCode { get; init; }

    /// <summary>Kullanıcıya/loga gösterilebilecek insan-okur mesaj.</summary>
    [JsonPropertyOrder(3)]
    public string Message { get; init; } = string.Empty;

    /// <summary>Başarı durumunda asıl veri; hata/boş durumda null.</summary>
    [JsonPropertyOrder(4)]
    public T? Data { get; init; }

    /// <summary>Hata durumunda hata listesi (alan bazlı doğrulama dahil); başarıda null.</summary>
    [JsonPropertyOrder(5)]
    public IReadOnlyList<ApiError>? Errors { get; init; }

    public static ApiResponse<T> Ok(T? data, int statusCode = 200, string message = "İşlem başarılı") => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message,
        Data = data,
        Errors = null
    };

    public static ApiResponse<T> Fail(int statusCode, string message, IReadOnlyList<ApiError>? errors = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Message = message,
        Data = default,
        Errors = errors
    };
}

/// <summary>Veri döndürmeyen (void) yanıtlar ve hatalar için kısayol fabrika — `ApiResponse&lt;object&gt;` üretir.</summary>
public static class ApiResponse
{
    public static ApiResponse<object?> Ok(int statusCode = 200, string message = "İşlem başarılı")
        => ApiResponse<object?>.Ok(null, statusCode, message);

    public static ApiResponse<object?> Fail(int statusCode, string message, IReadOnlyList<ApiError>? errors = null)
        => ApiResponse<object?>.Fail(statusCode, message, errors);
}
