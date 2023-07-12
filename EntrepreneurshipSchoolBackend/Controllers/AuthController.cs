using System.Security.Claims;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// Provides methods for authorization.
/// </summary>
[Route("api/")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _context;

    /// <summary>
    /// Create the controller with a dependency injection of a database context.
    /// </summary>
    /// <param name="context"></param>
    public AuthController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// This method returns user login data.
    /// </summary>
    /// <returns>Json in format {login: ""}</returns>
    [Authorize]
    [HttpGet("userinfo")]
    public IActionResult get()
    {
        return new OkObjectResult(new { login = HttpContext.User.Claims.First().Value });
    }

    /// <summary>
    /// Authenticates a user. 
    /// </summary>
    /// <param name="model">User's login and pasword.</param>
    /// <returns>200 if logged in.</returns>
    [HttpPost("/login")]
    public async Task<IActionResult> login(AuthModel model)
    {
        
        // TODO: check for correct credentials in DataBase. 
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.login) };
        // создаем объект ClaimsIdentity
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        // установка аутентификационных куки
        await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return new OkResult();
    }

    /// <summary>
    /// Logges a user out.
    /// </summary>
    /// <returns>200 if success.</returns>
    [HttpPost("/logout")]
    public async Task<IActionResult> logout()
    {
        await Request.HttpContext.SignOutAsync();
        return new OkResult();
    }
}

/// <summary>
/// A user's auth data.
/// </summary>
/// <param name="login">A user's login.</param>
/// <param name="password">A user's password.</param>
public record AuthModel(string login, string password);