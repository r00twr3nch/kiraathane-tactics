using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Authentication.Common;

namespace KiraathaneTactics.Application.Features.Authentication.Rename;

/// <summary>Açık hesabın duvar adını değiştirir. Eski duvar satırları da yeni ada çekilir.</summary>
public sealed record RenameCommand : ICommand<AuthenticationResponse>
{
    /// <summary>Duvara yazılacak yeni ad. 2 ile 24 karakter.</summary>
    /// <example>Çaycı Nuri</example>
    public string FullName { get; init; } = string.Empty;
}
