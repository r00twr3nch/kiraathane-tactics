using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Features.Authentication.Common;

/// <summary>Authentication akışına ait standart hatalar (tek yerden yönetilir).</summary>
public static class AuthenticationErrors
{
    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Auth.EmailAlreadyExists", "Bu e-posta adresi zaten kayıtlı.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "E-posta veya parola hatalı.");

    public static readonly Error UserInactive =
        Error.Forbidden("Auth.UserInactive", "Hesabınız pasif durumda.");

    public static readonly Error NotSignedIn =
        Error.Unauthorized("Auth.NotSignedIn", "Önce kapıdan içeri gir.");
}
