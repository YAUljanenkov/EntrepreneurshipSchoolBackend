using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using EntrepreneurshipSchoolBackend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// This controller is responsible for operating endpoints of transactions.
/// </summary>
public class TransactionController : ControllerBase
{
    private readonly ApiDbContext _context;

    /// <summary>
    /// Create the controller with a dependency injection of a database context.
    /// </summary>
    /// <param name="context">Context required to work with a database.</param>
    public TransactionController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="learnerId"></param>
    /// <param name="transactionType"></param>
    /// <param name="dateFrom"></param>
    /// <param name="dateTo"></param>
    /// <param name="sortProperty"></param>
    /// <param name="sortOrder"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("/admin/transactions")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult GetAdminTransactions(int? learnerId, string? transactionType, string? dateFrom,
        string? dateTo, string? sortProperty, string? sortOrder, int? page, int? pageSize)
    {
        if (learnerId != null && !_context.Learner.Any(x => x.Id == learnerId))
        {
            return new NotFoundResult();
        }

        TransactionType? type = null;

        if (transactionType != null && !_context.TransactionTypes.Any(x => x.Name == transactionType))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (_context.TransactionTypes.Any(x => x.Name == transactionType))
        {
            type = _context.TransactionTypes.First(x => x.Name == transactionType);
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
        if (sortProperty != null && !typeof(Transaction).GetProperties().Select(x => x.Name.ToLower())
                .Contains(sortProperty.ToLower()) && sortProperty.ToLower() != "description")
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

        var query = _context.Transactions
            .Include(x => x.Learner)
            .Include(x => x.Type)
            .Where(x => learnerId == null || x.Learner.Id == learnerId)
            .Where(x => type == null || x.Type == type)
            .Where(x => dateFromValue == DateTime.MinValue || dateFromValue < x.Date)
            .Where(x => dateToValue == DateTime.MinValue || dateToValue > x.Date);

        query = sortProperty?.ToLower() switch
        {
            "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
            "comment" or "description" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Comment)
                : query.OrderBy(x => x.Comment),
            "date" => sortOrder == "desc" ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
            "sum" => sortOrder == "desc" ? query.OrderByDescending(x => x.Sum) : query.OrderBy(x => x.Sum),
            "type" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Type.Name)
                : query.OrderBy(x => x.Type.Name),
            "learner" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Learner.Surname)
                : query.OrderBy(x => x.Learner.Surname),
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
                .Select(x => new TransactionDTO(x))
        };

        return new OkObjectResult(result);
    }
}