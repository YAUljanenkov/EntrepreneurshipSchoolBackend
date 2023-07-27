using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using EntrepreneurshipSchoolBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using Claim = EntrepreneurshipSchoolBackend.Models.Claim;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// This controller is responsible for operating endpoints of claims.
/// </summary>
[ApiController]
public class ClaimController : ControllerBase
{
    private readonly ApiDbContext _context;
    
    /// <summary>
    /// Create the controller with a dependency injection of a database context.
    /// </summary>
    /// <param name="context">Context required to work with a database.</param>
    public ClaimController(ApiDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("/admin/claims")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult getAdminClaims(string claimType, string? claimStatus, int? lotNumber, int? learnerId,
        int? taskId, int? receiverId, string? dateFrom, string? dateTo, string? sortProperty, string? sortOrder,
        int? page, int? pageSize)
    {
        if (!new[] { "BuyingLot", "FailedDeadline", "PlacingLot", "Transfer" }.Contains(claimType))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (claimStatus != null && !new[] { "Waiting", "Approved", "Declined" }.Contains(claimStatus))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        DateTime dateFromValue = DateTime.MinValue, dateToValue = DateTime.MinValue;
        var ruRU = new CultureInfo("ru-RU");

        if (dateFrom != null &&
            !DateTime.TryParseExact(dateFrom, "dd.MM.yyyy", ruRU, DateTimeStyles.None, out dateFromValue))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        dateFromValue = dateFromValue.ToUniversalTime();
        if (dateTo != null && !DateTime.TryParseExact(dateTo, "dd.MM.yyyy", ruRU, DateTimeStyles.None, out dateToValue))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        dateToValue = dateToValue.ToUniversalTime();
        if (sortProperty != null && !new[] { "id", "learner", "name", "datetime", "date", "claimStatus", "sum" }.Contains(sortProperty.ToLower()))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (sortOrder != null && sortOrder is not ("asc" or "desc"))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (page is < 1)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (pageSize is < 1)
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }
        
        var query = _context.Claim
            .Include(x => x.Learner)
            .Include(x => x.Type)
            .Where(x => x.Type.Name == claimType)
            .Where(x => claimStatus == null || x.Status.Name == claimStatus)
            .Where(x => dateFromValue == DateTime.MinValue || dateFromValue < x.Date)
            .Where(x => dateToValue == DateTime.MinValue || dateToValue > x.Date);

        if (claimType == "BuyingLot")
        {
            query = query
                .Where(x => learnerId == null || x.Receiver != null && x.Receiver.Id == learnerId)
                .Where(x => lotNumber == null || x.Lot != null && x.Lot.Number == lotNumber);
        }
        
        if (claimType == "FailedDeadline")
        {
            query = query
                .Where(x => learnerId == null || x.Learner != null && x.Learner.Id == learnerId)
                .Where(x => taskId == null || x.Task != null && x.Task.Id == taskId);
        }
        
        if (claimType == "PlacingLot")
        {
            query = query
                .Where(x => learnerId == null || x.Learner != null && x.Learner.Id == learnerId)
                .Where(x => lotNumber == null || x.Lot != null && x.Lot.Number == lotNumber);
        }
        
        if (claimType == "Transfer")
        {
            query = query
                .Where(x => receiverId == null || x.Receiver != null && x.Receiver.Id == receiverId)
                .Where(x => lotNumber == null || x.Learner != null && x.Learner.Id == lotNumber);
        }
        
        query = sortProperty?.ToLower() switch
        {
            "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
            "date" or "datetime" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Date)
                : query.OrderBy(x => x.Date),
            "sum" => sortOrder == "desc" ? query.OrderByDescending(x => x.Sum) : query.OrderBy(x => x.Sum),
            "claimStatus" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Status.Name)
                : query.OrderBy(x => x.Type.Name),
            "learner" or "name" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Learner.Surname).ThenByDescending(x => x.Learner.Name)
                    .ThenByDescending(x => x.Learner.Lastname)
                : query.OrderBy(x => x.Learner.Surname).ThenBy(x => x.Learner.Name).ThenBy(x => x.Learner.Lastname),
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
                .AsEnumerable()
                .Select(x => new ClaimDTO(x))
        };

        return new OkObjectResult(result);
    }
}