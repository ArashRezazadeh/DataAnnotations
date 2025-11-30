using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataAnnotations.Data;
using DataAnnotations.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace DataAnnotations.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AccountController(UserManager<IdentityUser> userManager, JwtSettings jwtSettings) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> Register(RegisterDTO model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(new { message = "User registered successfully" });
        }

        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    [HttpPost("login-with-Cookie")]
    public async Task<IActionResult> LoginWithCookie(LoginDTO model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            // to use Cookies you have to enable cookie authentication in prgram.cs
            await SignInAsync(user);
            return Ok(new { message = "User logged in successfully" });

        }
        return Unauthorized(new { message = "Invalid login attempt" });
    }

    [HttpPost("login-with-jwt")]
    public async Task<IActionResult> LoginWithJwt(LoginDTO model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            // to use Jwt you have to enable cookie authentication in prgram.cs
            var token = GenerateJwtToken(user);
            return Ok(token);
        }
        return Unauthorized(new { message = "Invalid login attempt" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "User logged out successfully" });
    }



    [HttpDelete("delete/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string email)
    {
        var user = await userManager.FindByEmailAsync(email);


        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok(new { message = "User deleted successfully" });
        }

        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }


    [Authorize]
    [HttpGet("authtest-cookie")]
    public IActionResult AuthTestCookie()
    {
        return Ok("This endpoint is secured");
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("authtest-jwt")]
    public IActionResult AuthTestJwt()
    {
        return Ok("This endpoint is secured");
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Admin")]
    [HttpGet("auth-role-test")]
    public IActionResult AuthRoleTest()
    {
        return Ok("This endpoint is secured");
    }

    [HttpGet("L-users")]
    [Authorize(Policy = "UsernameStartsWithL")]
    public IActionResult LUsersOnly()
    {
        return Ok("Your username starts with 'L'!");
    }




    // Handle login with Cookies!
    private async Task SignInAsync(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        if (HttpContext.Request.Cookies.TryGetValue(".AspNetCore.Cookies", out var cookieValue))
        {
            // For debugging - log or inspect the cookie
            Console.WriteLine($"Auth Cookie: {cookieValue}");
            
            // Or in a web application, you might want to return it or store it temporarily
            // Be careful not to expose sensitive cookie data in production
        }
    }

    private async Task<string> GenerateJwtToken(IdentityUser user)
    {

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new InvalidOperationException("User does not have a valid username");
        }

        Console.WriteLine($"GenerateJwtToken: JwtSettings Key length: {jwtSettings.Key?.Length ?? 0}");
        if (string.IsNullOrEmpty(jwtSettings.Key))
        {
            throw new InvalidOperationException("JWT key is null or empty");
        }


        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };


        var userRoles = await userManager.GetRolesAsync(user);

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(jwtSettings.ExpirationInMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 


// NOTE: 
// These methods are actually built into ASP.NET Core authentication. They are extension methods 
// on HttpContext, defined in AuthenticationHttpContextExtensions and added in Microsoft.AspNetCore.Authentication;