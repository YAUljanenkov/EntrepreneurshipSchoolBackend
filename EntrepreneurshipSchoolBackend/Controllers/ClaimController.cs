using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Utility;
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

    /// <summary>
    /// Returns claims list to admin filtered by options.
    /// </summary>
    /// <param name="claimType">Search claim type. Available values : BuyingLot, FailedDeadline, PlacingLot, Transfer</param>
    /// <param name="claimStatus">Search claim status Available values : Waiting, Approved, Declined</param>
    /// <param name="lotNumber">Search lot number</param>
    /// <param name="learnerId">Learner id</param>
    /// <param name="taskId">Task id</param>
    /// <param name="receiverId">Receiver id</param>
    /// <param name="dateFrom">The beginning of the desired interval</param>
    /// <param name="dateTo">The end of the desired interval</param>
    /// <param name="sortProperty">Property of response to sort by. Available values:  id, learner, name, datetime, date, claimStatus, sum</param>
    /// <param name="sortOrder">Sorting order. Available values : asc, desc</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">The size of the page to be returned</param>
    /// <returns>A list of claims with pagination info.</returns>
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
        if (sortProperty != null &&
            !new[] { "id", "learner", "name", "datetime", "date", "claimStatus", "sum" }.Contains(
                sortProperty.ToLower()))
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

    /// <summary>
    /// Approve/reject claim
    /// </summary>
    /// <param name="response">An object with approve or reject data.</param>
    /// <returns>200</returns>
    [HttpPut("/admin/claims")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult reactToClaim(ClaimResponseDTO response)
    {
        if (!new[] { "Approve", "Reject" }.Contains(response.action))
        {
            return new BadRequestResult();
        }

        if (!_context.Claim.Any(x => x.Id == response.id))
        {
            return new NotFoundObjectResult("Claim not found.");
        }

        var claim = _context.Claim.First(x => x.Id == response.id);
        var status = response.action == "Approve"
            ? _context.ClaimStatuses.First(x => x.Name == "Approved")
            : _context.ClaimStatuses.First(x => x.Name == "Declined");
        claim.Status = status;

        if (response.fine != null && response.action == "Approve" && claim.Type.Name == "DeadlineFailed")
        {
            claim.Sum = response.fine;
            var transactionType = _context.TransactionTypes.First(x => x.Name == "FailedDeadline");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.FailedDeadline(claim), 
                Date = DateTime.Now, Learner = claim.Learner, Sum = response.fine ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Learner.Balance += response.fine ?? 0;
        }

        if (claim.Type.Name == "BuyingLot" && response.action == "Approve")
        {
            var transactionType = _context.TransactionTypes.First(x => x.Name == "SellLot");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.LotIncome(claim), 
                Date = DateTime.Now, Learner = claim.Receiver, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Receiver.Balance += claim.Sum ?? 0;
        }
        
        if (claim.Type.Name == "BuyingLot" && response.action == "Reject")
        {
            var transactionType = _context.TransactionTypes.First(x => x.Name == "BuyLot");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.ReturnLot(claim), 
                Date = DateTime.Now, Learner = claim.Learner, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Learner.Balance += claim.Sum ?? 0;
        }

        if (claim.Type.Name == "PlacingLot" && response.action == "Approve")
        {
            var number = _context.Lots.OrderByDescending(x => x.Number).FirstOrDefault()?.Number;
            number = number == null ? 1 : number + 1;
            var lot = new Lot
            {
                Title = claim.Title ?? "",
                Description = claim.Description ?? "",
                Terms = claim.Terms ?? "",
                Performer = claim.Performer,
                Learner = claim.Learner,
                Price = claim.Sum ?? 0,
                Number = number ?? 1
            };
            _context.Lots.Add(lot);
        }

        if (claim.Type.Name == "Transfer" && response.action == "Approve")
        {
            var transactionType = _context.TransactionTypes.First(x => x.Name == "TransferIncome");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.TransferIncome(claim), 
                Date = DateTime.Now, Learner = claim.Receiver, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Receiver.Balance += claim.Sum ?? 0;
        }
        
        if (claim.Type.Name == "Transfer" && response.action == "Reject")
        {
            var transactionType = _context.TransactionTypes.First(x => x.Name == "TransferOutcome");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.TransferOutcomeReject(claim), 
                Date = DateTime.Now, Learner = claim.Learner, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Learner.Balance += claim.Sum ?? 0;
        }
        _context.SaveChanges();
        return new OkResult();
    }

    /// <summary>
    /// Get claim info by id
    /// </summary>
    /// <param name="id">Claim id</param>
    /// <returns>Claim object.</returns>
    [HttpGet("/admin/claims/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult getClaimById(int id)
    {
        if (!_context.Claim.Any(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        var claim = _context.Claim
            .Include(x => x.Learner)
            .Include(x => x.Receiver)
            .Include(x => x.Type)
            .Include(x => x.Task)
            .Include(x => x.Status)
            .First(x => x.Id == id);
        return new OkObjectResult(new ClaimInfoDTO(claim));
    }
}