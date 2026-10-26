using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Features.Authentication.Common;

/// <summary>Authentication akışına ait standart hatalar (tek yerden yönetilir).</summary>
public static class AuthenticationErrors
{
    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Auth.EmailAlreadyExists", "Bu e-posta adresi zaten kayıtlı.");

    public static readonly Error NameAlreadyExists =
        Error.Conflict("Auth.NameAlreadyExists", "Bu kullanıcı adı alınmış.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Kullanıcı adı veya parola hatalı.");

    public static readonly Error UserInactive =
        Error.Forbidden("Auth.UserInactive", "Hesabınız pasif durumda.");

    public static readonly Error NotSignedIn =
        Error.Unauthorized("Auth.NotSignedIn", "Önce kapıdan içeri gir.");

    public static readonly Error NameLocked =
        Error.Conflict("Auth.NameLocked", "Kullanıcı adı kayıtta yazılır, sonradan değişmez.");
}
