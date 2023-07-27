using System;
using System.IO;

using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EntrepreneurshipSchoolBackend.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public AccountController (ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/accounts")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetAccounts([FromBody] AccountsComplexRequest request)
        {
            var relevant_data = from m in _context.Learner
                                select m;
            if (request.name != null)
            {
                string surname = request.name.Split()[0];
                string name = request.name.Split()[1];
                relevant_data = relevant_data.Where(x =>  x.Name.Equals(name) && x.Surname.Equals(surname));
            }
            if (request.email != null)
            {
                relevant_data = relevant_data.Where(x => x.EmailLogin.Equals(request.email));
            }
            if (request.role != null)
            {
                char role = request.role == "Learner" ? '0' : '1';
                relevant_data = relevant_data.Where(x => x.IsTracker == role);
            }
            if (request.team != null)
            {
                relevant_data = relevant_data.Where(x => x.Relate != null
                                                        && x.Relate.Any(x=> x.Group != null && x.Group.Number == request.team));
            }
            if (request.sortProperty != null)
            {
                if (request.sortOrder == "desc")
                {
                    switch (request.sortProperty)
                    {
                        case "name":
                            relevant_data = from m in relevant_data
                                            orderby m.Name descending
                                            select m;
                            break;
                        case "email":
                            relevant_data = from m in relevant_data
                                            orderby m.EmailLogin descending
                                            select m;
                            break;
                        case "id":
                            relevant_data = from m in relevant_data
                                            orderby m.Id descending
                                            select m;
                            break;
                        case "role":
                            relevant_data = from m in relevant_data
                                            orderby m.IsTracker descending
                                            select m;
                            break;
                        case "balance":
                            relevant_data = from m in relevant_data
                                            orderby m.Balance descending
                                            select m;
                            break;
                        default:
                            return BadRequest("Invalid sortby parametr");

                    }
                } else
                {
                    switch (request.sortProperty)
                    {
                        case "name":
                            relevant_data = from m in relevant_data
                                            orderby m.Name
                                            select m;
                            break;
                        case "email":
                            relevant_data = from m in relevant_data
                                            orderby m.EmailLogin 
                                            select m;
                            break;
                        case "id":
                            relevant_data = from m in relevant_data
                                            orderby m.Id 
                                            select m;
                            break;
                        case "role":
                            relevant_data = from m in relevant_data
                                            orderby m.IsTracker 
                                            select m;
                            break;
                        case "balance":
                            relevant_data = from m in relevant_data
                                            orderby m.Balance
                                            select m;
                            break;
                        default:
                            return BadRequest("Invalid sortby parametr");

                    }
                }
            }
            var accsOnThePage = relevant_data.Skip(request.pageSize * (request.page - 1)).Take(request.pageSize);
            if (accsOnThePage == null)
            {
                return BadRequest("page number is too big");
            }
            List<AccountInfo> content = new List<AccountInfo>();
            foreach (var acc in accsOnThePage)
            {
                AccountInfo info = new AccountInfo();
                info.email = acc.EmailLogin;
                info.role = acc.IsTracker == '0' ? "Learner" : "Tracker";
                info.balance = acc.Balance;
                info.id = acc.Id;
                info.name = acc.Surname + " " + acc.Name;
                var TeamNumbers = from r in _context.Relates
                                  where r.LearnerId == acc.Id
                                  select r.Group.Number;
                info.teamNumber = TeamNumbers.ToList();
                content.Add(info);
            }
            Pagination pagination = new Pagination();
            pagination.total_elements = relevant_data.Count();
            pagination.total_pages = (relevant_data.Count() + request.pageSize - 1) / request.pageSize;
            pagination.pageSize = content.Count;
            pagination.page_number = request.page;
            AccountComplexResponse response = new AccountComplexResponse();
            response.content = content;
            response.pagination = pagination;
            return Ok(response);
        }

        [HttpPost("/admin/accounts")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> CreateAccount([FromBody] UserRequest user)
        {
            List<string> same_properties = new List<string>();
            var intersect = _context.Learner.FirstOrDefault(ob => ob.EmailLogin == user.email);
            if (intersect != null)
            {
                same_properties.Add("email");
            }
            intersect = _context.Learner.FirstOrDefault(ob => ob.Phone == user.phone);
            if (intersect != null)
            {
                same_properties.Add("phone");
            }
            if (same_properties.Count > 0)
            {
                return StatusCode(409, same_properties);
            }
            if (user.role !=  Roles.Tracker && user.role != Roles.Learner)
            {
                return BadRequest("bad role");
            }

            Learner newLearner = new Learner();
            newLearner.Name = user.name;
            newLearner.Surname = user.surname;
            newLearner.Lastname = user.middleName;
            newLearner.Messenger = user.messenger;
            newLearner.EmailLogin = user.email;
            newLearner.Phone = user.phone;
            newLearner.Password = user.password;
            newLearner.IsTracker = user.role == Roles.Learner ? '0' : '1';
            if (user.gender != null)
            {
                newLearner.Gender = (bool)user.gender ? '1' : '0';
            }
            newLearner.Balance = 0;
            newLearner.ResultGrade = 0;
            newLearner.GradeBonus = 0;

            _context.Learner.Add(newLearner);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPut("/admin/accounts")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateAccount([FromBody] UserRequest user)
        {
            if (user.id == null)
            {
                return BadRequest("Id is not specified");
            }
            Models.Learner? acc = _context.Learner.Find(user.id);
            if (acc == null)
            {
                return NotFound();
            }
            if (user.role != Roles.Tracker && user.role != Roles.Learner)
            {
                return BadRequest("bad role");
            }

            acc.Name = user.name;
            acc.Surname = user.surname;
            acc.Lastname = user.middleName;
            acc.Messenger = user.messenger;
            acc.EmailLogin = user.email;
            acc.Phone = user.phone;
            acc.Password = user.password;
            acc.IsTracker = user.role == Roles.Learner ? '0' : '1';
            if (user.gender != null)
            {
                acc.Gender = (bool)user.gender ? '1' : '0';
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/accounts/{id}")]
        public async Task<ActionResult> GetAccountPublicData(int id)
        {
            Models.Learner? acc = await _context.Learner.FindAsync(id);
            if (acc == null) { return NotFound();}
            ExtendedAccountInfo info = new ExtendedAccountInfo();
            info.email = acc.EmailLogin;
            info.role = acc.IsTracker == '0' ? "Learner" : "Tracker";
            info.balance = acc.Balance;
            info.id = acc.Id;
            info.fullName = acc.Surname + " " + acc.Name;
            var TeamNumbers = from r in _context.Relates
                              where r.LearnerId == acc.Id
                              select r.Group.Number;
            info.teamNumber = TeamNumbers.ToList();
            info.phone = acc.Phone;
            if (acc.Messenger != null) { 
                info.messenger = acc.Messenger;
            }
            if (acc.Gender != null)
            {
                info.gender = acc.Gender == '1';
            }
            return Ok(info);
        }

        [HttpGet("/learner/accounts/balance-name")]
        public async Task<ActionResult> GetAccountBalance()
        {
            string? string_id = HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value;
            if (string_id == null)
            {
                return BadRequest();
            }
            int user_id = int.Parse(string_id);
            Models.Learner? acc = await _context.Learner.FindAsync(user_id);
            if (acc == null) { return NotFound(); }
            BalanceNameResponse response = new BalanceNameResponse();
            response.name = acc.Surname + " " + acc.Name;
            response.balance = acc.Balance;
            return Ok(response);
        }

        [HttpDelete("/admin/accounts/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteAccount(int id)
        {
            Models.Learner? acc = await _context.Learner.FindAsync(id);
            if (acc == null)
            {
                return NotFound();
            }

            _context.Learner.Remove(acc);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/admin/accounts/select")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetAccListByRole([FromBody] string? role)
        {
            if (role == null)
            {
                return BadRequest("role must be specified");
            }
            if (role != "Learner" && role != "Tracker") { return BadRequest("bad role"); }

            
            char role_c = role == "Learner" ? '0' : '1';
            var relevant_data = from m in _context.Learner
                                where m.IsTracker == role_c
                                select m;
            
            List<AccountShortenInfo> content = new List<AccountShortenInfo>();
            foreach (var acc in relevant_data)
            {
                AccountShortenInfo info = new AccountShortenInfo();
                info.id = acc.Id;
                info.name = acc.Surname + " " + acc.Name;
                content.Add(info);
            }

            return Ok(content);
        }

    }
}
