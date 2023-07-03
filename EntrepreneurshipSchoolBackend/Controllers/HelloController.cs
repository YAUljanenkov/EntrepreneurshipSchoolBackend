using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EntrepreneurshipSchoolBackend.Controllers;

[Route("api/hello")]
[ApiController]
public class HelloController: ControllerBase
{
    private readonly ApiDbContext _context;

    public HelloController(ApiDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Test method to see if everything is working.
    /// </summary>
    /// <returns>Hello world.</returns>
    [HttpGet("")]
    public IActionResult GetClub()
    {
        
        return new OkObjectResult(_context.Lessons.ToList());
    }
}