using FluentValidation;

namespace KiraathaneTactics.Application.Features.Products.Update;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode("Product.Id.Required").WithMessage("Ürün kimliği zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("Product.Name.Required").WithMessage("Ürün adı zorunludur.")
            .MaximumLength(200).WithErrorCode("Product.Name.TooLong").WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithErrorCode("Product.Description.TooLong").WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithErrorCode("Product.Price.Negative").WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithErrorCode("Product.Stock.Negative").WithMessage("Stok negatif olamaz.");
    }
}
