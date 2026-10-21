namespace KiraathaneTactics.Domain.Enums;

/// <summary>Kıraathane jokerleri. Oyuncu başına maçta toplam üç hak.</summary>
public enum PerkType
{
    /// <summary>Konan taşı kilitler; çevrilemez.</summary>
    DemirAtma = 0,

    /// <summary>Taşı kapalı koyar. Sahibin sonraki hamlesine kadar savaşmaz ve savaşılmaz, sonra açılıp savaşır.</summary>
    GizliHamle = 1,

    /// <summary>Süre kısma niyeti. Saat Phase 3'te uygulanır; burada yalnızca hak düşer.</summary>
    LafEbeligi = 2
}
