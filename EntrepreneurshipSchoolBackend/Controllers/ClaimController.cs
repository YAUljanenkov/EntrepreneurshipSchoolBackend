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
    public async Task<IActionResult> getAdminClaims(string claimType, string? claimStatus, int? lotNumber,
        int? learnerId,
        int? taskId, int? receiverId, DateTime? dateFrom, DateTime? dateTo, string? sortProperty, string? sortOrder,
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
        
        var dateFromValue = dateFrom?.ToUniversalTime();
        var dateToValue = dateTo?.ToUniversalTime();
        
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
            .Include(x => x.Status)
            .Include(x => x.Lot)
            .Include(x => x.Receiver)
            .Include(x => x.Task)
            .Where(x => x.Type.Name == claimType)
            .Where(x => claimStatus == null || x.Status.Name == claimStatus)
            .Where(x => dateFromValue == DateTime.MinValue || dateFromValue < x.Date)
            .Where(x => dateToValue == DateTime.MinValue || dateToValue > x.Date);

        switch (claimType)
        {
            case "BuyingLot":
                query = query
                    .Where(x => learnerId == null || x.Receiver != null && x.Receiver.Id == learnerId)
                    .Where(x => lotNumber == null || x.Lot != null && x.Lot.Number == lotNumber);
                break;
            case "FailedDeadline":
                query = query
                    .Where(x => learnerId == null || x.Learner != null && x.Learner.Id == learnerId)
                    .Where(x => taskId == null || x.Task != null && x.Task.Id == taskId);
                break;
            case "PlacingLot":
                query = query
                    .Where(x => learnerId == null || x.Learner != null && x.Learner.Id == learnerId)
                    .Where(x => lotNumber == null || x.Lot != null && x.Lot.Number == lotNumber);
                break;
            case "Transfer":
                query = query
                    .Where(x => receiverId == null || x.Receiver != null && x.Receiver.Id == receiverId)
                    .Where(x => lotNumber == null || x.Learner != null && x.Learner.Id == lotNumber);
                break;
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
    public async Task<IActionResult> reactToClaim(ClaimResponseDTO response)
    {
        if (!new[] { "Approve", "Reject" }.Contains(response.action))
        {
            return new BadRequestResult();
        }

        if (!_context.Claim.Any(x => x.Id == response.id))
        {
            return new NotFoundObjectResult("Claim not found.");
        }

        var claim = await _context.Claim
            .Include(x => x.Type)
            .Include(x => x.Learner)
            .Include(x => x.Status)
            .Include(x => x.Task)
            .Include(x => x.Receiver)
            .Include(x => x.Lot)
            .FirstAsync(x => x.Id == response.id);

        if (claim.Status.Name != "Waiting")
        {
            return new ObjectResult("Claim already approved or rejected.") { StatusCode = 409 };
        }

        var status = response.action == "Approve"
            ? await _context.ClaimStatuses.FirstAsync(x => x.Name == "Approved")
            : await _context.ClaimStatuses.FirstAsync(x => x.Name == "Declined");
        claim.Status = status;

        if (response.fine != null && response.action == "Approve" && claim.Type.Name == "DeadlineFailed")
        {
            claim.Sum = response.fine;
            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "FailedDeadline");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.FailedDeadline(claim),
                Date = DateTime.Now.ToUniversalTime(), Learner = claim.Learner, Sum = -response.fine ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Learner.Balance += response.fine ?? 0;
        }

        if (claim.Type.Name == "BuyingLot" && response.action == "Approve")
        {
            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "SellLot");
            if (claim.Lot != null)
            {
                var lotLearner = (await _context.Lots.Include(x => x.Learner).FirstAsync(x => claim.Lot.Id == x.Id))
                    .Learner;
                if (lotLearner != null)
                {
                    var transaction = new Transaction
                    {
                        Type = transactionType, Claim = claim, Comment = TransactionComments.LotIncome(claim),
                        Date = DateTime.Now.ToUniversalTime(), Learner = lotLearner, Sum = claim.Sum ?? 0
                    };
                    _context.Transactions.Add(transaction);
                    lotLearner.Balance += claim.Sum ?? 0;
                }
            }
        }

        if (claim.Type.Name == "BuyingLot" && response.action == "Reject")
        {
            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "BuyLot");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.ReturnLot(claim),
                Date = DateTime.Now.ToUniversalTime(), Learner = claim.Learner, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Learner.Balance += claim.Sum ?? 0;
        }

        if (claim.Type.Name == "PlacingLot" && response.action == "Approve")
        {
            var number = (await _context.Lots.OrderByDescending(x => x.Number).FirstOrDefaultAsync())?.Number;
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
            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "TransferIncome");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.TransferIncome(claim),
                Date = DateTime.Now.ToUniversalTime(), Learner = claim.Receiver, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Receiver.Balance += claim.Sum ?? 0;
        }

        if (claim.Type.Name == "Transfer" && response.action == "Reject")
        {
            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "TransferOutcome");
            var transaction = new Transaction
            {
                Type = transactionType, Claim = claim, Comment = TransactionComments.TransferOutcomeReject(claim),
                Date = DateTime.Now.ToUniversalTime(), Learner = claim.Learner, Sum = claim.Sum ?? 0
            };
            _context.Transactions.Add(transaction);
            claim.Learner.Balance += claim.Sum ?? 0;
        }

        await _context.SaveChangesAsync();
        return new OkResult();
    }

    /// <summary>
    /// Get claim info by id
    /// </summary>
    /// <param name="id">Claim id</param>
    /// <returns>Claim object.</returns>
    [HttpGet("/admin/claims/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> getClaimById(int id)
    {
        if (!await _context.Claim.AnyAsync(x => x.Id == id))
        {
            return new NotFoundResult();
        }

        var claim = await _context.Claim
            .Include(x => x.Learner)
            .Include(x => x.Receiver)
            .Include(x => x.Type)
            .Include(x => x.Task)
            .Include(x => x.Status)
            .Include(x => x.Lot)
            .FirstAsync(x => x.Id == id);
        return new OkObjectResult(new ClaimInfoDTO(claim));
    }

    /// <summary>
    /// Get number of new claims by type
    /// </summary>
    /// <returns>The response contains an array of objects, each of which corresponds to one of the request types and contains the number of requests of this type with the status = Waiting.</returns>
    [HttpGet("/admin/claims/new-amount")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> getWaitingClaimsAmount()
    {
        return new OkObjectResult(await _context.ClaimTypes.Select(claimType => new
        {
            claimType = claimType.Name,
            amount = _context.Claim.Count(x => x.Type == claimType && x.Status.Name == "Waiting")
        }).ToListAsync());
    }

    /// <summary>
    /// Get list of claims by filter and sort.
    /// </summary>
    /// <param name="claimType">Search claim type. Available values : BuyingLot, FailedDeadline, PlacingLot, Transfer</param>
    /// <param name="claimStatus">Search claim status. Available values : Waiting, Approved, Declined</param>
    /// <param name="dateFrom">The beginning of the desired interval</param>
    /// <param name="dateTo">The end of the desired interval</param>
    /// <param name="sortProperty">Property of response to sort by. Available values : id, learner or name, datetime or date, claimStatus, sum</param>
    /// <param name="sortOrder">Sorting order. Available values : asc, desc</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">The size of the page to be returned</param>
    /// <returns>list of claims with pagination info.</returns>
    [HttpGet("/learner/claims")]
    [Authorize(Roles = Roles.Learner)]
    public async Task<IActionResult> getLearnerClaims(string? claimType, string? claimStatus, DateTime? dateFrom,
        DateTime? dateTo,
        string? sortProperty, string? sortOrder, int? page, int? pageSize)
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

        if (claimType != null && !new[] { "BuyingLot", "FailedDeadline", "PlacingLot", "Transfer" }.Contains(claimType))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        if (claimStatus != null && !new[] { "Waiting", "Approved", "Declined" }.Contains(claimStatus))
        {
            return new BadRequestObjectResult("Incorrect parameters.");
        }

        var dateFromValue = dateFrom?.ToUniversalTime();
        var dateToValue = dateTo?.ToUniversalTime();
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
            .Include(x => x.Status)
            .Include(x => x.Lot)
            .Where(x => x.Learner == learner)
            .Where(x => claimType == null || x.Type.Name == claimType)
            .Where(x => claimStatus == null || x.Status.Name == claimStatus)
            .Where(x => dateFromValue == DateTime.MinValue || dateFromValue < x.Date)
            .Where(x => dateToValue == DateTime.MinValue || dateToValue > x.Date);


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
                .Select(x => new ClaimDTO(x))
        };

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Create claim by user.
    /// </summary>
    /// <param name="newClaim">Object with new claim data.</param>
    /// <returns>200</returns>
    [HttpPost("/learner/claims")]
    [Authorize(Roles = Roles.Learner)]
    public async Task<IActionResult> createClaim(CreateClaimDTO newClaim)
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

        if (!new[] { "BuyingLot", "PlacingLot", "Transfer" }.Contains(newClaim.claimType))
        {
            return new BadRequestObjectResult("Some of the crucial properties have not been specified");
        }

        var claimStatus = await _context.ClaimStatuses.FirstAsync(x => x.Name == "Waiting");
        if (newClaim.claimType == "BuyingLot")
        {
            if (newClaim.buyingLotId == null)
            {
                return new BadRequestObjectResult("Some of the crucial properties have not been specified");
            }

            if (!await _context.Lots.AnyAsync(x => x.Id == newClaim.buyingLotId))
            {
                return new NotFoundObjectResult("Lot not found.");
            }

            var lot = await _context.Lots.FirstAsync(x => x.Id == newClaim.buyingLotId);
            var claimType = await _context.ClaimTypes.FirstAsync(x => x.Name == "BuyingLot");
            var claim = new Claim
            {
                Learner = learner,
                Lot = lot,
                Type = claimType,
                Status = claimStatus,
                Sum = lot.Price,
                Date = DateTime.Now.ToUniversalTime()
            };

            if (lot.Price > learner.Balance)
            {
                return new ObjectResult("Not enough money") { StatusCode = 403 };
            }

            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "BuyLot");
            var transaction = new Transaction
            {
                Type = transactionType,
                Claim = claim,
                Date = DateTime.Now.ToUniversalTime(),
                Learner = learner,
                Comment = TransactionComments.BuyLot(claim),
                Sum = -lot.Price
            };

            learner.Balance -= lot.Price;

            _context.Claim.Add(claim);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        if (newClaim.claimType == "PlacingLot")
        {
            if (newClaim.lot == null)
            {
                return new BadRequestObjectResult("Some of the crucial properties have not been specified");
            }

            var claimType = await _context.ClaimTypes.FirstAsync(x => x.Name == "PlacingLot");
            var claim = new Claim
            {
                Learner = learner,
                Title = newClaim.lot.name,
                Description = newClaim.lot.description,
                Terms = newClaim.lot.terms,
                Sum = newClaim.lot.price,
                Type = claimType,
                Status = claimStatus,
                Date = DateTime.Now.ToUniversalTime()
            };

            _context.Claim.Add(claim);
            await _context.SaveChangesAsync();
        }

        if (newClaim.claimType == "Transfer")
        {
            if (newClaim.receiverId == null || newClaim.sum == null)
            {
                return new BadRequestObjectResult("Some of the crucial properties have not been specified");
            }

            if (!await _context.Learner.AnyAsync(x => x.Id == newClaim.receiverId))
            {
                return new NotFoundObjectResult("Receiver not found.");
            }

            if (newClaim.sum > learner.Balance)
            {
                return new ObjectResult("Not enough money") { StatusCode = 403 };
            }

            var receiver = await _context.Learner.FirstAsync(x => x.Id == newClaim.receiverId);

            var claimType = await _context.ClaimTypes.FirstAsync(x => x.Name == "Transfer");

            var claim = new Claim
            {
                Learner = learner,
                Sum = newClaim.sum,
                Type = claimType,
                Status = claimStatus,
                Date = DateTime.Now.ToUniversalTime(),
                Receiver = receiver
            };
            var transactionType = await _context.TransactionTypes.FirstAsync(x => x.Name == "TransferOutcome");
            var transaction = new Transaction
            {
                Type = transactionType,
                Claim = claim,
                Date = DateTime.Now.ToUniversalTime(),
                Learner = learner,
                Comment = TransactionComments.TransferOutcome(claim),
                Sum = -newClaim.sum ?? 0
            };

            learner.Balance -= newClaim.sum ?? 0;

            _context.Claim.Add(claim);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        return new OkResult();
    }
}