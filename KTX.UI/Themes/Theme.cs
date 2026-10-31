using System.Drawing;

namespace KTX.UI.Themes;

public enum ThemeMode
{
    Light,
    Dark
}

public class Theme
{
    public ThemeMode Mode { get; init; }

    // Nền & Khung
    public Color Background { get; init; }
    public Color Surface { get; init; }
    public Color SurfaceHover { get; init; }
    public Color SurfaceCard { get; init; }
    public Color Border { get; init; }
    public Color BorderHover { get; init; }

    // Màu chữ
    public Color TextPrimary { get; init; }
    public Color TextSecondary { get; init; }
    public Color TextMuted { get; init; }

    // Màu thương hiệu & Trạng thái
    public Color Primary { get; init; }
    public Color PrimaryHover { get; init; }
    public Color PrimaryPressed { get; init; }
    public Color PrimaryLight { get; init; }
    public Color PrimaryText { get; init; }

    public Color Success { get; init; }
    public Color SuccessBg { get; init; }
    public Color SuccessText { get; init; }

    public Color Warning { get; init; }
    public Color WarningBg { get; init; }
    public Color WarningText { get; init; }

    public Color Danger { get; init; }
    public Color DangerBg { get; init; }
    public Color DangerText { get; init; }

    public Color Info { get; init; }
    public Color InfoBg { get; init; }
    public Color InfoText { get; init; }

    public static Theme LightTheme => new()
    {
        Mode = ThemeMode.Light,
        Background = ColorTranslator.FromHtml("#F5F7FB"),
        Surface = ColorTranslator.FromHtml("#FFFFFF"),
        SurfaceHover = ColorTranslator.FromHtml("#F0F3F9"),
        SurfaceCard = ColorTranslator.FromHtml("#FFFFFF"),
        Border = ColorTranslator.FromHtml("#E3E8EF"),
        BorderHover = ColorTranslator.FromHtml("#CBD5E1"),

        TextPrimary = ColorTranslator.FromHtml("#111827"),
        TextSecondary = ColorTranslator.FromHtml("#4B5563"),
        TextMuted = ColorTranslator.FromHtml("#9CA3AF"),

        Primary = ColorTranslator.FromHtml("#2563EB"),
        PrimaryHover = ColorTranslator.FromHtml("#1D4ED8"),
        PrimaryPressed = ColorTranslator.FromHtml("#1E40AF"),
        PrimaryLight = ColorTranslator.FromHtml("#EFF6FF"),
        PrimaryText = Color.White,

        Success = ColorTranslator.FromHtml("#16A34A"),
        SuccessBg = ColorTranslator.FromHtml("#DCFCE7"),
        SuccessText = ColorTranslator.FromHtml("#15803D"),

        Warning = ColorTranslator.FromHtml("#F59E0B"),
        WarningBg = ColorTranslator.FromHtml("#FEF3C7"),
        WarningText = ColorTranslator.FromHtml("#B45309"),

        Danger = ColorTranslator.FromHtml("#DC2626"),
        DangerBg = ColorTranslator.FromHtml("#FEE2E2"),
        DangerText = ColorTranslator.FromHtml("#B91C1C"),

        Info = ColorTranslator.FromHtml("#0284C7"),
        InfoBg = ColorTranslator.FromHtml("#E0F2FE"),
        InfoText = ColorTranslator.FromHtml("#0369A1")
    };

    public static Theme DarkTheme => new()
    {
        Mode = ThemeMode.Dark,
        Background = ColorTranslator.FromHtml("#0F1115"),
        Surface = ColorTranslator.FromHtml("#181B21"),
        SurfaceHover = ColorTranslator.FromHtml("#22262E"),
        SurfaceCard = ColorTranslator.FromHtml("#181B21"),
        Border = ColorTranslator.FromHtml("#2A2F38"),
        BorderHover = ColorTranslator.FromHtml("#3B424F"),

        TextPrimary = ColorTranslator.FromHtml("#F3F4F6"),
        TextSecondary = ColorTranslator.FromHtml("#9CA3AF"),
        TextMuted = ColorTranslator.FromHtml("#6B7280"),

        Primary = ColorTranslator.FromHtml("#3B82F6"),
        PrimaryHover = ColorTranslator.FromHtml("#2563EB"),
        PrimaryPressed = ColorTranslator.FromHtml("#1D4ED8"),
        PrimaryLight = ColorTranslator.FromHtml("#1E293B"),
        PrimaryText = Color.White,

        Success = ColorTranslator.FromHtml("#22C55E"),
        SuccessBg = ColorTranslator.FromHtml("#052E16"),
        SuccessText = ColorTranslator.FromHtml("#4ADE80"),

        Warning = ColorTranslator.FromHtml("#FBBF24"),
        WarningBg = ColorTranslator.FromHtml("#451A03"),
        WarningText = ColorTranslator.FromHtml("#FCD34D"),

        Danger = ColorTranslator.FromHtml("#EF4444"),
        DangerBg = ColorTranslator.FromHtml("#450A0A"),
        DangerText = ColorTranslator.FromHtml("#FCA5A5"),

        Info = ColorTranslator.FromHtml("#38BDF8"),
        InfoBg = ColorTranslator.FromHtml("#082F49"),
        InfoText = ColorTranslator.FromHtml("#7DD3FC")
    };
}
