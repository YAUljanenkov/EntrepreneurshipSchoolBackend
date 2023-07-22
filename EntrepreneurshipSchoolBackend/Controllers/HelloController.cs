using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EntrepreneurshipSchoolBackend.Controllers;

[Route("api/hello")]
[ApiController]
public class HelloController : ControllerBase
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

    [HttpGet("GetAssessmentsTypes")]
    public ActionResult GetAssessmentsTypes()
    {
        return Ok(_context.AssessmentsTypes.ToList());
    }

    [HttpGet("GetClaimTypes")]
    public ActionResult GetClaimTypes() 
    { 
        return Ok(_context.ClaimTypes.ToList());
    }

    [HttpGet("GetTaskTypes")]
    public ActionResult GetTaskTypes()
    {
        return Ok(_context.TaskTypes.ToList());
    }

    [HttpGet("GetTransactionTypes")]
    public ActionResult GetTransactionTypes()
    {
        return Ok(_context.TransactionTypes.ToList());
    }

    [HttpGet("GetFinalGradeTypes")]
    public ActionResult GetFinalGradeTypes()
    {
        return Ok(_context.FinalTypes.ToList());
    }
}