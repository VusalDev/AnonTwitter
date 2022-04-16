namespace YETwitter.Identity.Web.Configuration;

public class JwtOptions
{
    [Required]
    public string ValidAudience { get; set; }

    [Required]
    public string ValidIssuer { get; set; }

    [Required]
    public string Secret { get; set; }

    public int TokenLifetimeMinutes { get; set; } = 60;

    public int PermanentTokenLifetimeDays { get; set; } = 30;
}
