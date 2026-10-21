namespace KiraathaneTactics.Domain.Exceptions;

/// <summary>
/// Domain iş kurallarının ihlal edildiği durumlarda fırlatılır.
/// Her ihlalin kendine has bir <see cref="Code"/> değeri vardır (örn. "Product.NameRequired"),
/// böylece istemci hatayı programatik olarak ayırt edebilir.
/// </summary>
public class DomainException(string message, string code = "Domain.RuleViolation") : Exception(message)
{
    /// <summary>Makine-okur, benzersiz hata kodu.</summary>
    public string Code { get; } = code;
}
