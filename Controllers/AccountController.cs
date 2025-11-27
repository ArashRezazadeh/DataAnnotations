using System.Security.Claims;
using DataAnnotations.Data;
using DataAnnotations.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataAnnotations.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AccountController(UserManager<IdentityUser> userManager) : ControllerBase
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            await SignInAsync(user);
            return Ok(new { message = "User logged in successfully" });
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
    [HttpGet("authtest")]
    public IActionResult AuthTest()
    {
        return Ok("This endpoint is secured");
    }


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
} 


// NOTE: 
// These methods are actually built into ASP.NET Core authentication. They are extension methods 
// on HttpContext, defined in AuthenticationHttpContextExtensions and added in Microsoft.AspNetCore.Authentication;