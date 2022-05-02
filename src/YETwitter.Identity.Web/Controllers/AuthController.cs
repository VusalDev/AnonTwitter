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

[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class AuthController : AuthControllerBase
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


    public override async Task<ActionResult<TokenDataModel>> Login([FromBody] LoginModel? model, CancellationToken cancellationToken = default)
    {
        model = model ?? throw new ArgumentNullException(nameof(model));

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    //new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims, model.RememberMe);

            return Ok(new TokenDataModel
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ValidTo = token.ValidTo
            });
        }
        return Unauthorized();
    }

    public override async Task<ActionResult<ResponseModel>> Register([FromBody] RegisterModel? model, CancellationToken cancellationToken = default)
    {
        model = model ?? throw new ArgumentNullException(nameof(model));

        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "User creation failed",
                detail: "User already exists"
            );

        var user = new IdentityUser()
        {
            UserName = model.Username,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "User creation failed",
                detail: result.Errors.FirstOrDefault()?.Description
            );

        return Ok(new ResponseModel { Status = "Success", Message = "User created successfully" });
    }

    [Authorize]
    [HttpPost]
    [Route("change-password")]
    public override async Task<ActionResult<ResponseModel>> ChangePassword([FromBody] ChangePasswordModel model, CancellationToken cancellationToken = default)
    {
        model = model ?? throw new ArgumentNullException(nameof(model));

        var username = HttpContext.User?.Identity?.Name ?? throw new InvalidOperationException("User not logged in");
        var user = await _userManager.FindByNameAsync(username);
        var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
        if (!result.Succeeded)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Password changing failed",
                detail: result.Errors.FirstOrDefault()?.Description
            );

        return Ok(new ResponseModel { Status = "Success", Message = "Password changed successfully" });
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