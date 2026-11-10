namespace Identity.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "JwtOptions";

    public string SecretKey { get; set; } = "EnterpriseECommercePlatformUltraSecureJwtSecretKey2026!#*";
    public string Issuer { get; set; } = "ECommercePlatform.IdentityService";
    public string Audience { get; set; } = "ECommercePlatform";
    public int ExpirationMinutes { get; set; } = 60;
}
