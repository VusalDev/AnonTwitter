using System.ComponentModel.DataAnnotations;

namespace YETwitter.ApiGateway.Web.Configuration;

public class CorsOptions
{
    public const string AppDefaultPolicy = "AllowAll";

    [Required]
    public string[] Origins { get; set; } = new string[0];
}
