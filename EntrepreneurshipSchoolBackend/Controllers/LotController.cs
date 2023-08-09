using System.Security.Claims;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntrepreneurshipSchoolBackend.DTOs.Lots;
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
    /// <param name="performerOther">Param to search lots with unregistered performer</param>
    /// <param name="sortProperty">Property of response to sort by</param>
    /// <param name="sortOrder">Sorting order</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">The size of the page to be returned</param>
    /// <returns>List of lots with pagination info.</returns>
    [HttpGet("/admin/lots")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetLotsAdmin(int? lotNumber, string? lotTitle, int? learnerId,
        string? performerOther,
        string? sortProperty, string? sortOrder, int? page, int? pageSize)
    {
        if (sortOrder != null && sortOrder.ToLower() is not ("asc" or "desc"))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (sortProperty != null &&
            !new[] { "id", "title", "description", "number", "price", "terms", "performer", "learner" }.Contains(
                sortProperty.ToLower()))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (learnerId != null && performerOther != null)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (lotNumber != null && !await _context.Lots.AnyAsync(x => x.Number == lotNumber))
        {
            return new NotFoundResult();
        }

        if (learnerId != null && !await _context.Learner.AnyAsync(x => x.Id == learnerId))
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
            .Where(x => performerOther == null || x.Performer != null && x.Performer.Contains(performerOther))
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

        var size = await query.CountAsync();

        var result = new
        {
            pagination = new
            {
                page = page ?? 1,
                pageSize = pageSize ?? 10,
                totalPages = Math.Ceiling(size / (double)(pageSize ?? 10)),
                totalElements = size
            },
            content = (await query
                    .Skip(page != null && pageSize != null ? ((int)page - 1) * (int)pageSize : 0)
                    .Take(pageSize ?? 10)
                    .ToListAsync())
                .Select(x => new LotShortInfoDTO(x))
        };

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Create lot.
    /// </summary>
    /// <param name="newCreateLot">Lot to create.</param>
    /// <returns>200</returns>
    [HttpPost("/admin/lots")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateLot(CreateLotDTO newCreateLot)
    {
        var number = (await _context.Lots.OrderByDescending(x => x.Number).FirstOrDefaultAsync())?.Number + 1 ?? 1;
        var lot = new Lot
        {
            Title = newCreateLot.title,
            Description = newCreateLot.description,
            Terms = newCreateLot.terms,
            Performer = newCreateLot.performer,
            Price = newCreateLot.price,
            Number = number
        };

        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();
        return new OkResult();
    }

    /// <summary>
    /// Update lot.
    /// </summary>
    /// <param name="newLotData">Lot data.</param>
    /// <returns>200</returns>
    [HttpPut("/admin/lots")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateLot(UpdateLotDTO newLotData)
    {
        if (!await _context.Lots.AnyAsync(x => x.Id == newLotData.id))
        {
            return new NotFoundResult();
        }

        var lot = await _context.Lots.FirstAsync(x => x.Id == newLotData.id);
        lot.Title = newLotData.title ?? lot.Title;
        lot.Description = newLotData.description ?? lot.Description;
        lot.Terms = newLotData.terms ?? lot.Terms;
        lot.Price = newLotData.price ?? lot.Price;
        lot.Performer = newLotData.performer ?? lot.Performer;
        await _context.SaveChangesAsync();
        return new OkResult();
    }

    /// <summary>
    /// Get lot info by id
    /// </summary>
    /// <param name="id">Lot id</param>
    /// <returns>Lot info</returns>
    [HttpGet("/admin/lots/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetLotById(int id)
    {
        if (!await _context.Lots.AnyAsync(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(
            new LotInfoDTO(await _context.Lots.Include(x => x.Learner).FirstAsync(x => x.Id == id)));
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
        if (!await _context.Lots.AnyAsync(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        var lot = await _context.Lots.Include(x => x.Learner).FirstAsync(x => x.Id == id);
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
    public async Task<IActionResult> GetLotsByLearner(int? lotNumber, string? lotTitle, int? priceFrom, int? priceTo,
        int? page, int? pageSize)
    {
        if (!int.TryParse(HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value, out int learnerId))
        {
            return new BadRequestResult();
        }

        if (!await _context.Learner.AnyAsync(x => x.Id == learnerId))
        {
            return new UnauthorizedResult();
        }

        var learner = await _context.Learner.FirstAsync(x => x.Id == learnerId);

        if (lotNumber != null && !await _context.Lots.AnyAsync(x => x.Number == lotNumber))
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
            .Where(x => x.Learner == learner)
            .Where(x => lotTitle == null || x.Title.Contains(lotTitle ?? ""))
            .Where(x => priceFrom == null || x.Price >= priceFrom)
            .Where(x => priceTo == null || x.Price <= priceTo);

        var size = await query.CountAsync();

        var result = new
        {
            pagination = new
            {
                page = page ?? 1,
                pageSize = pageSize ?? 10,
                totalPages = Math.Ceiling(size / (double)(pageSize ?? 10)),
                totalElements = size
            },
            content = (await query
                    .Skip(page != null && pageSize != null ? ((int)page - 1) * (int)pageSize : 0)
                    .Take(pageSize ?? 10)
                    .ToListAsync())
                .Select(x => new LotInfoDTO(x))
        };

        return new OkObjectResult(result);
    }
}