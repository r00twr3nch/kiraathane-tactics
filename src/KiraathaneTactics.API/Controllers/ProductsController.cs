using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Features.Products.Common;
using KiraathaneTactics.Application.Features.Products.Create;
using KiraathaneTactics.Application.Features.Products.Delete;
using KiraathaneTactics.Application.Features.Products.GetById;
using KiraathaneTactics.Application.Features.Products.GetList;
using KiraathaneTactics.Application.Features.Products.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiraathaneTactics.API.Controllers;

/// <summary>
/// Ürün yönetimi (örnek CRUD). Listeleme ve detay anonim; oluştur/güncelle kimlik doğrulaması,
/// silme ise yalnızca <b>Admin</b> rolü ister.
/// </summary>
[Authorize]
public sealed class ProductsController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Ürünleri sayfalı ve opsiyonel arama ile listeler.</summary>
    /// <remarks>
    /// Sayfalama alanları query string ile gönderilir.
    /// <c>pageSize</c> en fazla 100'dür; aşan değerler 100'e çekilir.
    ///
    /// Örnek: <c>GET /api/products?pageNumber=1&amp;pageSize=10&amp;search=laptop</c>
    /// </remarks>
    /// <param name="query">Sayfalama (pageNumber, pageSize) ve arama (search) parametreleri.</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="200">Sayfalı ürün listesi.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(query, cancellationToken));

    /// <summary>Tek bir ürünü kimliğine (Id) göre getirir.</summary>
    /// <param name="id">Ürünün benzersiz kimliği (GUID).</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="200">Ürün bulundu.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(new GetProductByIdQuery(id), cancellationToken));

    /// <summary>Yeni ürün oluşturur.</summary>
    /// <remarks>
    /// Kimlik doğrulaması gerekir (<c>Authorize</c>). Başarılıysa 201 ve oluşturulan kaydın Id'si döner.
    ///
    /// Örnek istek gövdesi:
    ///
    ///     POST /api/products
    ///     {
    ///        "name": "Örnek Ürün",
    ///        "description": "Kısa açıklama",
    ///        "price": 99.90,
    ///        "stockQuantity": 25
    ///     }
    /// </remarks>
    /// <param name="command">Oluşturulacak ürün bilgileri.</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="201">Ürün oluşturuldu; Location başlığı ve yeni Id döner.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="401">Kimlik doğrulaması gerekli.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleCreated(result, nameof(GetById), new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>Var olan ürünü günceller.</summary>
    /// <remarks>
    /// Route'taki <c>id</c> ile gövdedeki <c>id</c> aynı olmalıdır.
    ///
    /// Örnek istek gövdesi:
    ///
    ///     PUT /api/products/{id}
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "name": "Güncel Ürün",
    ///        "description": "Yeni açıklama",
    ///        "price": 149.90,
    ///        "stockQuantity": 10
    ///     }
    /// </remarks>
    /// <param name="id">Güncellenecek ürünün kimliği.</param>
    /// <param name="command">Güncel ürün bilgileri.</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="200">Güncelleme başarılı.</response>
    /// <response code="400">Doğrulama hatası veya id uyuşmazlığı.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ErrorResponse(StatusCodes.Status400BadRequest,
                "Route'taki id ile gövdedeki id uyuşmuyor.", "IdMismatch");

        return HandleResult(await Sender.Send(command, cancellationToken));
    }

    /// <summary>Ürünü siler (soft delete — kayıt fiziksel olarak silinmez).</summary>
    /// <param name="id">Silinecek ürünün kimliği.</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="200">Silme başarılı.</response>
    /// <response code="401">Kimlik doğrulaması gerekli.</response>
    /// <response code="403">Yetki yetersiz (Admin rolü gerekir).</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(new DeleteProductCommand(id), cancellationToken));
}
