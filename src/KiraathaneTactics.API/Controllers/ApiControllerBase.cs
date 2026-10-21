using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace KiraathaneTactics.API.Controllers;

/// <summary>
/// Tüm controller'ların tabanı. ISender erişimi + Result'ı tek tip <see cref="ApiResponse{T}"/>
/// zarfına çeviren yardımcılar sağlar. Controller'lar iş mantığı içermez.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected ISender Sender { get; } = sender;

    /// <summary>Değer döndüren sonuç: başarıda 200 + data, hatada uygun durum kodu + errors.</summary>
    protected IActionResult HandleResult<T>(Result<T> result)
        => result.IsSuccess
            ? Ok(ApiResponse<T>.Ok(result.Value))
            : Failure(result.Error);

    /// <summary>Değer döndürmeyen sonuç: başarıda 200 (data: null), hatada uygun durum kodu + errors.</summary>
    protected IActionResult HandleResult(Result result)
        => result.IsSuccess
            ? Ok(ApiResponse.Ok())
            : Failure(result.Error);

    /// <summary>Kaynak oluşturma: başarıda 201 + data + Location header.</summary>
    protected IActionResult HandleCreated<T>(Result<T> result, string actionName, object routeValues)
        => result.IsSuccess
            ? CreatedAtAction(
                actionName,
                routeValues,
                ApiResponse<T>.Ok(result.Value, StatusCodes.Status201Created, "Kayıt oluşturuldu"))
            : Failure(result.Error);

    /// <summary>Ad-hoc hata yanıtı (örn. controller içi basit doğrulamalar).</summary>
    protected IActionResult ErrorResponse(int statusCode, string message, string code = "Error")
        => new ObjectResult(ApiResponse.Fail(statusCode, message, [new ApiError(code, null, message)]))
        {
            StatusCode = statusCode
        };

    private static IActionResult Failure(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        var body = ApiResponse.Fail(
            statusCode,
            error.Description,
            [new ApiError(error.Code, null, error.Description)]);

        return new ObjectResult(body) { StatusCode = statusCode };
    }
}
