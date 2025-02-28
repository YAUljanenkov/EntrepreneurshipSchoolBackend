using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using EntrepreneurshipSchoolBackend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// This controller is responsible for operating endpoints of transactions.
/// </summary>
[ApiController]
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
    /// Get transactions with filters by admin.
    /// </summary>
    /// <param name="learnerId">Id of a learner.</param>
    /// <param name="transactionType">Types of transactions. Supported types: Activity, SellLot, AdminIncome, TransferIncome, FailedDeadline, BuyLot, AdminOutcome, TransferOutcome</param>
    /// <param name="dateFrom">The beginning of the desired interval</param>
    /// <param name="dateTo">The end of the desired interval</param>
    /// <param name="sortProperty">Property of response to sort by. Supported: id, type, date, sum, comment or description, learner.</param>
    /// <param name="sortOrder">Sorting order. Available values : asc, desc</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">The size of the page to be returned</param>
    /// <returns>A list of transactions with pagination info.</returns>
    [HttpGet("/admin/transactions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAdminTransactions(int? learnerId, string? transactionType, DateTime? dateFrom,
        DateTime? dateTo, string? sortProperty, string? sortOrder, int? page, int? pageSize)
    {
        if (learnerId != null && !await _context.Learner.AnyAsync(x => x.Id == learnerId))
        {
            return new NotFoundResult();
        }

        TransactionType? type = null;

        if (transactionType != null && !await _context.TransactionTypes.AnyAsync(x => x.Name == transactionType))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (await _context.TransactionTypes.AnyAsync(x => x.Name == transactionType))
        {
            type = await _context.TransactionTypes.FirstAsync(x => x.Name == transactionType);
        }
        var dateFromValue = dateFrom?.ToUniversalTime();
        var dateToValue = dateTo?.ToUniversalTime();
        if (sortProperty != null && !typeof(Transaction).GetProperties().Select(x => x.Name.ToLower())
                .Contains(sortProperty.ToLower()) && sortProperty.ToLower() != "description" &&
            sortProperty.ToLower() != "dateTime")
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
            "date" or "datetime" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Date)
                : query.OrderBy(x => x.Date),
            "sum" => sortOrder == "desc" ? query.OrderByDescending(x => x.Sum) : query.OrderBy(x => x.Sum),
            "type" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Type.Name)
                : query.OrderBy(x => x.Type.Name),
            "learner" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Learner.Surname).ThenByDescending(x => x.Learner.Name)
                    .ThenByDescending(x => x.Learner.Lastname)
                : query.OrderBy(x => x.Learner.Surname).ThenBy(x => x.Learner.Name).ThenBy(x => x.Learner.Lastname),
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
                .Select(x => new TransactionDTO(x))
        };

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Creates new transaction by admin.
    /// </summary>
    /// <param name="transaction">A transaction to create.</param>
    /// <returns>200 if all data is OK and transaction created.</returns>
    [HttpPost("/admin/transactions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> createTransaction(CreateTransaction transaction)
    {
        if (!await _context.Learner.AnyAsync(x => x.Id == transaction.learnerId))
        {
            return new BadRequestResult();
        }

        var learner = await _context.Learner.FirstAsync(x => x.Id == transaction.learnerId);
        var type = await _context.TransactionTypes.FirstAsync(
            x => x.Name == (transaction.sum > 0 ? "AdminIncome" : "AdminOutcome"));
        var newTransaction = new Transaction
        {
            Comment = transaction.description,
            Sum = transaction.sum,
            Learner = learner,
            Type = type,
            Date = DateTime.Now.ToUniversalTime()
        };

        _context.Transactions.Add(newTransaction);
        learner.Balance += transaction.sum;
        await _context.SaveChangesAsync();
        return new OkResult();
    }

    /// <summary>
    /// Get transaction by id for admin.
    /// </summary>
    /// <param name="id">Id of a transaction.</param>
    /// <returns>A transaction.</returns>
    [HttpGet("/admin/transactions/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> getTransaction(int id)
    {
        if (!await _context.Transactions.AnyAsync(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(new TransactionDTO(await _context.Transactions.Include(x => x.Learner).Include(x => x.Claim).Include(x => x.Type)
            .FirstAsync(x => x.Id == id)));
    }

    /// <summary>
    /// Get transactions with filters by learner.
    /// </summary>
    /// <param name="transactionType">Types of transactions. Supported types: Activity, SellLot, AdminIncome, TransferIncome, FailedDeadline, BuyLot, AdminOutcome, TransferOutcome</param>
    /// <param name="dateFrom">The beginning of the desired interval</param>
    /// <param name="dateTo">The end of the desired interval</param>
    /// <param name="sortProperty">Property of response to sort by. Supported: id, type, date, sum, comment or description, learner.</param>
    /// <param name="sortOrder">Sorting order. Available values : asc, desc</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">The size of the page to be returned</param>
    /// <returns>A list of transactions with pagination info.</returns>
    [HttpGet("/learner/transactions")]
    [Authorize(Roles = Roles.Learner)]
    public async Task<IActionResult> GetLearnerTransactions(string? transactionType, DateTime? dateFrom,
        DateTime? dateTo, string? sortProperty, string? sortOrder, int? page, int? pageSize)
    {
        if (!int.TryParse(HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value, out int learnerId))
        {
            return new UnauthorizedResult();
        }

        if (!await _context.Learner.AnyAsync(x => x.Id == learnerId))
        {
            return new UnauthorizedResult();
        }

        return await GetAdminTransactions(learnerId, transactionType, dateFrom, dateTo, sortProperty, sortOrder, page,
            pageSize);
    }
}