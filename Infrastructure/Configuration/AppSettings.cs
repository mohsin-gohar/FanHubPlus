using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration for the application with validation.
/// </summary>
public class AppSettings
{
    public const string SectionName = "AppSettings";

    [Required]
    public string SiteName { get; set; } = "FanHubPlus";

    [Required]
    public string SiteUrl { get; set; } = "https://localhost";

    public string SupportEmail { get; set; } = "support@fanhubplus.com";

    public int DefaultPageSize { get; set; } = 20;

    public int MaxPageSize { get; set; } = 100;

    public bool EnableDemoData { get; set; } = false;

    public bool EnableChatbot { get; set; } = true;

    public bool EnableGames { get; set; } = true;

    public bool EnableMerchandise { get; set; } = true;

    public bool EnableFanArt { get; set; } = true;

    public GameSettings Games { get; set; } = new();

    public CacheSettings Cache { get; set; } = new();

    public SecuritySettings Security { get; set; } = new();

    public ExternalApiSettings ExternalApis { get; set; } = new();
}

/// <summary>
/// Gaming module configuration.
/// </summary>
public class GameSettings
{
    public string GamesPath { get; set; } = "/games/";

    public int MaxUploadSizeMB { get; set; } = 500;

    public string[] AllowedExtensions { get; set; } = { ".html", ".js", ".wasm", ".zip", ".json", ".css", ".png", ".jpg", ".webp" };

    public bool EnableIframeSandbox { get; set; } = true;

    public string[] AllowedIframeDomains { get; set; } = { "itch.io", "www.youtube.com", "player.vimeo.com" };

    public int DefaultWidth { get; set; } = 1280;

    public int DefaultHeight { get; set; } = 720;

    public bool EnableGamepadSupport { get; set; } = true;

    public bool EnableCloudSaves { get; set; } = true;

    public TimeSpan CloudSaveInterval { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Cache configuration.
/// </summary>
public class CacheSettings
{
    public bool EnableRedis { get; set; } = true;

    public string RedisConnectionString { get; set; } = "localhost:6379";

    public int DefaultTTLMinutes { get; set; } = 30;

    public int ShortTTLMinutes { get; set; } = 5;

    public int LongTTLHours { get; set; } = 24;

    public bool EnableResponseCaching { get; set; } = true;

    public int ResponseCacheTTLSeconds { get; set; } = 60;
}

/// <summary>
/// Security configuration.
/// </summary>
public class SecuritySettings
{
    public bool EnableCsp { get; set; } = true;

    public bool EnableHsts { get; set; } = true;

    public int HstsMaxAgeDays { get; set; } = 365;

    public bool EnableXFrameOptions { get; set; } = true;

    public string XFrameOptions { get; set; } = "SAMEORIGIN";

    public bool EnableReferrerPolicy { get; set; } = true;

    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    public bool EnablePermissionsPolicy { get; set; } = true;

    public string PermissionsPolicy { get; set; } = "geolocation=(), microphone=(), camera=(), fullscreen=(self)";

    public bool EnableCspNonce { get; set; } = true;

    public string[] CspScriptSrc { get; set; } = { "'self'", "https://cdn.jsdelivr.net", "https://cdnjs.cloudflare.com" };

    public string[] CspStyleSrc { get; set; } = { "'self'", "'unsafe-inline'", "https://fonts.googleapis.com" };

    public string[] CspFontSrc { get; set; } = { "'self'", "https://fonts.gstatic.com" };

    public string[] CspImgSrc { get; set; } = { "'self'", "data:", "https:" };

    public string[] CspFrameSrc { get; set; } = { "https://www.youtube.com", "https://player.vimeo.com", "https://itch.io" };

    public string[] CspConnectSrc { get; set; } = { "'self'", "https://api.fanhubplus.com" };
}

/// <summary>
/// External API configuration.
/// </summary>
public class ExternalApiSettings
{
    public YouTubeSettings YouTube { get; set; } = new();

    public VimeoSettings Vimeo { get; set; } = new();

    public ItchIoSettings ItchIo { get; set; } = new();
}

public class YouTubeSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string EmbedBaseUrl { get; set; } = "https://www.youtube.com/embed/";

    public bool EnableNoCookie { get; set; } = true;

    public string NoCookieDomain { get; set; } = "youtube-nocookie.com";
}

public class VimeoSettings
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string EmbedBaseUrl { get; set; } = "https://player.vimeo.com/video/";
}

public class ItchIoSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string EmbedBaseUrl { get; set; } = "https://itch.io/embed/";

    public bool AllowCustomGames { get; set; } = true;
}