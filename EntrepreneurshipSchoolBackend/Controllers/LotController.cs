using System.Security.Claims;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntrepreneurshipSchoolBackend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// This controller is responsible for operating endpoints of transactions.
/// </summary>
[ApiController]
public class LotController : ControllerBase
{
    private readonly ApiDbContext _context;

    /// <summary>
    /// Create the controller with a dependency injection of a database context.
    /// </summary>
    /// <param name="context">Context required to work with a database.</param>
    public LotController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get list of lots by filter and sort.
    /// </summary>
    /// <param name="lotNumber">Search lot number</param>
    /// <param name="lotTitle">Search lot title</param>
    /// <param name="learnerId">Learner id</param>
    /// <param name="sortProperty">Property of response to sort by</param>
    /// <param name="sortOrder">Sorting order</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">The size of the page to be returned</param>
    /// <returns>List of lots with pagination info.</returns>
    [HttpGet("/admin/lots")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult GetLotsAdmin(int? lotNumber, string? lotTitle, int? learnerId, string? sortProperty,
        string? sortOrder, int? page, int? pageSize)
    {
        if (sortOrder != null && !new[] { "asc", "desc" }.Contains(sortOrder.ToLower()))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (sortProperty != null &&
            !new[] { "id", "title", "description", "number", "price", "terms", "performer", "learner" }.Contains(
                sortProperty.ToLower()))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (lotNumber != null && !_context.Lots.Any(x => x.Number == lotNumber))
        {
            return new NotFoundResult();
        }

        if (learnerId != null && !_context.Learner.Any(x => x.Id == learnerId))
        {
            return new NotFoundResult();
        }

        if (page is < 1)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (pageSize is < 1)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        var query = _context.Lots
            .Include(x => x.Learner)
            .Where(x => lotNumber == null || x.Number == lotNumber)
            .Where(x => learnerId == null || x.Learner != null && x.Learner.Id == learnerId)
            .Where(x => lotTitle == null || x.Title.Contains(lotTitle ?? ""));

        query = sortProperty?.ToLower() switch
        {
            "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
            "title" => sortOrder == "desc" ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "description" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Description)
                : query.OrderBy(x => x.Description),
            "number" => sortOrder == "desc" ? query.OrderByDescending(x => x.Number) : query.OrderBy(x => x.Number),
            "price" => sortOrder == "desc" ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "terms" => sortOrder == "desc" ? query.OrderByDescending(x => x.Terms) : query.OrderBy(x => x.Terms),
            "performer" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Performer)
                : query.OrderBy(x => x.Performer),
            "learner" => sortOrder == "desc"
                ? query.Where(x => x.Learner != null).OrderByDescending(x => x.Learner.Surname)
                    .ThenByDescending(x => x.Learner.Name).ThenByDescending(x => x.Learner.Lastname)
                : query.Where(x => x.Learner != null).OrderBy(x => x.Learner.Surname).ThenBy(x => x.Learner.Name)
                    .ThenBy(x => x.Learner.Lastname),
            _ => query
        };

        var size = query.Count();

        var result = new
        {
            pagination = new
            {
                page = page ?? 1,
                pageSize = pageSize ?? 10,
                totalPages = Math.Ceiling(size / (double)(pageSize ?? 10)),
                totalElements = size
            },
            content = query
                .Skip(page != null && pageSize != null ? ((int)page - 1) * (int)pageSize : 0)
                .Take(pageSize ?? 10)
        };

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Create lot.
    /// </summary>
    /// <param name="newLot">Lot to create.</param>
    /// <returns>200</returns>
    [HttpPost("/admin/lots")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult CreateLot(LotDTO newLot)
    {
        var number = _context.Lots.OrderByDescending(x => x.Number).FirstOrDefault()?.Number + 1 ?? 1;
        var lot = new Lot
        {
            Title = newLot.title,
            Description = newLot.description,
            Terms = newLot.terms,
            Performer = newLot.performer,
            Price = newLot.price,
            Number = number
        };

        _context.Lots.Add(lot);
        _context.SaveChanges();
        return new OkResult();
    }

    /// <summary>
    /// Update lot.
    /// </summary>
    /// <param name="newLotData">Lot data.</param>
    /// <returns>200</returns>
    [HttpPut("/admin/lots")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult UpdateLot(UpdateLotDTO newLotData)
    {
        if (!_context.Lots.Any(x => x.Id == newLotData.id))
        {
            return new NotFoundResult();
        }

        var lot = _context.Lots.First(x => x.Id == newLotData.id);
        lot.Title = newLotData.title ?? lot.Title;
        lot.Description = newLotData.description ?? lot.Description;
        lot.Terms = newLotData.terms ?? lot.Terms;
        lot.Price = newLotData.price ?? lot.Price;
        lot.Performer = newLotData.performer ?? lot.Performer;
        _context.SaveChanges();
        return new OkResult();
    }

    /// <summary>
    /// Get lot info by id
    /// </summary>
    /// <param name="id">Lot id</param>
    /// <returns>Lot info</returns>
    [HttpGet("/admin/lots/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult GetLotById(int id)
    {
        if (!_context.Lots.Any(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(_context.Lots.First(x => x.Id == id));
    }
    
    /// <summary>
    /// Delete lot by id.
    /// </summary>
    /// <param name="id">Lot id.</param>
    /// <returns>200</returns>
    [HttpDelete("/admin/lots/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteLotById(int id)
    {
        if (!_context.Lots.Any(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        var lot = _context.Lots.Include(x => x.Learner).First(x => x.Id == id);
        await _context.Claim.Where(x => x.Lot == lot).ForEachAsync(x => x.Lot = null);
        lot.Learner = null;
        _context.Lots.Remove(lot);
        await _context.SaveChangesAsync();
        return new OkResult();
    }

    /// <summary>
    /// Get list of lots by filter and sort.
    /// </summary>
    /// <param name="lotNumber">Search lot number.</param>
    /// <param name="lotTitle">Search lot title.</param>
    /// <param name="priceFrom">The beginning of the desired interval.</param>
    /// <param name="priceTo">The end of the desired interval.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">The size of the page to be returned.</param>
    /// <returns>List of lots with pagination info.</returns>
    [HttpGet("/learner/lots/")]
    [Authorize(Roles = Roles.Learner)]
    public IActionResult GetLotsByLearner(int? lotNumber, string? lotTitle, int? priceFrom, int? priceTo, int? page, int? pageSize)
    {
        if (!int.TryParse(HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value, out int learnerId))
        {
            return new BadRequestResult();
        }

        if (!_context.Learner.Any(x => x.Id == learnerId))
        {
            return new UnauthorizedResult();
        }

        var learner = _context.Learner.First(x => x.Id == learnerId);
        
        if (lotNumber != null && !_context.Lots.Any(x => x.Number == lotNumber))
        {
            return new NotFoundResult();
        }

        if (page is < 1)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (pageSize is < 1)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        var query = _context.Lots
            .Include(x => x.Learner)
            .Where(x => lotNumber == null || x.Number == lotNumber)
            .Where(x =>  x.Learner == learner)
            .Where(x => lotTitle == null || x.Title.Contains(lotTitle ?? ""))
            .Where(x => priceFrom == null || x.Price >= priceFrom)
            .Where(x => priceTo == null || x.Price <= priceTo);
        
        var size = query.Count();

        var result = new
        {
            pagination = new
            {
                page = page ?? 1,
                pageSize = pageSize ?? 10,
                totalPages = Math.Ceiling(size / (double)(pageSize ?? 10)),
                totalElements = size
            },
            content = query
                .Skip(page != null && pageSize != null ? ((int)page - 1) * (int)pageSize : 0)
                .Take(pageSize ?? 10)
        };

        return new OkObjectResult(result);
    }
}