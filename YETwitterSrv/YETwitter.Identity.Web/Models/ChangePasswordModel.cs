namespace YETwitter.Identity.Web.Models;

public class ChangePasswordModel
{
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
}

