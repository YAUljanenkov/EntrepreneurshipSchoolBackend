using System.Security.Claims;
using EntrepreneurshipSchoolBackend.Models;
using EntrepreneurshipSchoolBackend.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Claim = System.Security.Claims.Claim;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// Provides methods for authorization.
/// </summary>
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _context;

    /// <summary>
    /// Create the controller with a dependency injection of a database context.
    /// </summary>
    /// <param name="context">Context required to work with a database.</param>
    public AuthController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// This method returns user login data.
    /// </summary>
    /// <returns>An array of claim data.</returns>
    [Authorize]
    [HttpGet("userinfo")]
    public IActionResult get()
    {
        var dict = new List<string[]>();
        foreach (var claim in HttpContext.User.Claims)
        {
            dict.Add(new[] { claim.Type, claim.Value });
        }

        return new OkObjectResult(dict);
    }

    /// <summary>
    /// Authenticates a user. 
    /// </summary>
    /// <param name="model">User's login and pasword.</param>
    /// <returns>200 if logged in, 404 if not.</returns>
    [HttpPost("/auth")]
    public async Task<IActionResult> login(AuthModel model)
    {
        var claims = new List<Claim>();
        string role;
        if (await _context.Admins.AnyAsync(x => x.EmailLogin == model.login))
        {
            var admin = await _context.Admins.FirstAsync(x => x.EmailLogin == model.login);
            if (!Hashing.VerifyHashedPassword(admin.Password, model.password))
            {
                return NotFound();
            }

            claims.Add(new Claim(ClaimTypes.Sid, admin.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));
            role = Roles.Admin;
        }
        else if (await _context.Learner.AnyAsync(x => x.EmailLogin == model.login))
        {
            var learner = await _context.Learner.FirstAsync(x => x.EmailLogin == model.login);
            if (!Hashing.VerifyHashedPassword(learner.Password, model.password))
            {
                return NotFound();
            }

            claims.Add(new Claim(ClaimTypes.Sid, learner.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, Roles.Learner));
            role = Roles.Learner;
            if (learner.IsTracker == '1')
            {
                claims.Add(new Claim(ClaimTypes.Role, Roles.Tracker));
                role = Roles.Tracker;
            }
        }
        else
        {
            return NotFound();
        }

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return new OkObjectResult(new { role });
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