using System.Collections;
using System.Security.Claims;
using System.Security.Cryptography;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        if (_context.Admins.Any(x => x.EmailLogin == model.login))
        {
            var admin = _context.Admins.First(x => x.EmailLogin == model.login);
            if (!VerifyHashedPassword(admin.Password, model.password))
            {
                return NotFound();
            }

            claims.Add(new Claim(ClaimTypes.Sid, admin.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));
            role = Roles.Admin;
        }
        else if (_context.Learner.Any(x => x.EmailLogin == model.login))
        {
            var learner = _context.Learner.First(x => x.EmailLogin == model.login);
            if (!VerifyHashedPassword(learner.Password, model.password))
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

    /// <summary>
    /// Hashes a password with salt and SHA512 algorithm.
    /// </summary>
    /// <param name="password">A string password to hash.</param>
    /// <returns>A hashed password.</returns>
    /// <exception cref="ArgumentNullException">Throws if a password is null.</exception>
    public static string HashPassword(string password)
    {
        byte[] salt;
        byte[] buffer2;
        if (password == null)
        {
            throw new ArgumentNullException(password);
        }

        using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8, HashAlgorithmName.SHA512))
        {
            salt = bytes.Salt;
            buffer2 = bytes.GetBytes(0x20);
        }

        byte[] dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
        Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
        return Convert.ToBase64String(dst);
    }

    /// <summary>
    /// Verifies that a password equals to hash.
    /// </summary>
    /// <param name="hashedPassword">A hash of a password.</param>
    /// <param name="password">A password to check.</param>
    /// <returns>True if passwords are equal.</returns>
    /// <exception cref="ArgumentNullException">Throws if a password to check is null.</exception>
    public static bool VerifyHashedPassword(string? hashedPassword, string password)
    {
        byte[] buffer4;
        if (hashedPassword == null)
        {
            return false;
        }

        if (password == null)
        {
            throw new ArgumentNullException("password");
        }

        byte[] src = Convert.FromBase64String(hashedPassword);
        if ((src.Length != 0x31) || (src[0] != 0))
        {
            return false;
        }

        byte[] dst = new byte[0x10];
        Buffer.BlockCopy(src, 1, dst, 0, 0x10);
        byte[] buffer3 = new byte[0x20];
        Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
        using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8, HashAlgorithmName.SHA512))
        {
            buffer4 = bytes.GetBytes(0x20);
        }

        return buffer3.SequenceEqual(buffer4);
    }
}

/// <summary>
/// A user's auth data.
/// </summary>
/// <param name="login">A user's login.</param>
/// <param name="password">A user's password.</param>
public record AuthModel(string login, string password);