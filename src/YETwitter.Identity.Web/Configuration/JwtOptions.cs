namespace YETwitter.Identity.Web.Configuration;

public class JwtOptions
{
    [Required]
    public string ValidAudience { get; set; } = default!;

    [Required]
    public string ValidIssuer { get; set; } = default!;

    [Required]
    public string Secret { get; set; } = default!;

    public int TokenLifetimeMinutes { get; set; } = 60;

    public int PermanentTokenLifetimeDays { get; set; } = 30;
}
