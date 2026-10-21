using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Features.Products.Common;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"'{id}' kimlikli ürün bulunamadı.");
}
