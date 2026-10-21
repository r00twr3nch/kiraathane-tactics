namespace KiraathaneTactics.Application.Common.Results;

/// <summary>
/// Bir işlemin sonucunu (başarı/başarısızlık) exception fırlatmadan ifade eder.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("Başarılı sonuçta hata olamaz.");
            case false when error == Error.None:
                throw new InvalidOperationException("Başarısız sonuçta hata zorunludur.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>Başarı durumunda bir değer (Value) taşıyan sonuç.</summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    /// <summary>Yalnızca başarılı sonuçta okunabilir; aksi halde exception fırlatır.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Başarısız sonucun değeri okunamaz.");

    // T -> Result<T> ve Error -> Result<T> örtük dönüşümleri (kullanım kolaylığı)
    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
