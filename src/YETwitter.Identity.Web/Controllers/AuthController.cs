using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using YETwitter.Identity.Web.Configuration;
using YETwitter.Identity.Web.Models;

namespace YETwitter.Identity.Web.Controllers;

[Route("api/v1/auth")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        UserManager<IdentityUser> userManager,
        IOptions<JwtOptions> jwtOpts)
    {
        _userManager = userManager;
        _jwtOptions = jwtOpts.Value;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims, model.RememberMe);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
        return Unauthorized();
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "User creation failed",
                detail: "User already exists!"
            );

        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "User creation failed",
                detail: $"User creation failed! {result.Errors.FirstOrDefault()?.Description}"
            );

        return Ok(new ResponseModel { Status = "Success", Message = "User created successfully!" });
    }

    [Authorize]
    [HttpPost]
    [Route("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        var username = HttpContext.User?.Identity?.Name ?? throw new ArgumentNullException("username");
        var user = await _userManager.FindByNameAsync(username);
        var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
        if (!result.Succeeded)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Password changing failed",
                detail: $"Password changing failed! {result.Errors.FirstOrDefault()?.Description}"
            );

        return Ok(new ResponseModel { Status = "Success", Message = "Password changed successfully!" });
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims, bool isPermanent)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var expires = isPermanent ? DateTime.Now.AddDays(_jwtOptions.PermanentTokenLifetimeDays) : DateTime.Now.AddMinutes(_jwtOptions.TokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.ValidIssuer,
            audience: _jwtOptions.ValidAudience,
            expires: expires,
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }
}