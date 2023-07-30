using System;
using System.IO;

using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Security.Claims;
using EntrepreneurshipSchoolBackend.DTOs;
using System.Security.Cryptography.X509Certificates;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class TeamsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public TeamsController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/teams")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetTeams([FromBody] TeamsComplexRequest request)
        {
            var relevant_data = from m in _context.Groups
                                select m;
            if (request.teamNumber != null)
            {
                relevant_data = from m in relevant_data
                                where m.Number == request.teamNumber
                                select m;
            }

            if (request.sortProperty != null)
            {
                if (request.sortOrder == "desc")
                {
                    switch (request.sortProperty)
                    {
                        case "teamNumber":
                            relevant_data.OrderByDescending(x => x.Number);
                            break;
                        case "id":
                            relevant_data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            return BadRequest("Invalid sortby parametr");

                    }
                }
                else
                {
                    switch (request.sortProperty)
                    {
                        case "teamNumber":
                            relevant_data.OrderBy(x => x.Number);
                            break;
                        
                        case "id":
                            relevant_data.OrderBy(x => x.Id);
                            break;
                        default:
                            return BadRequest("Invalid sortby parametr");

                    }
                }
            }
            var teamsOnThePage = relevant_data.Skip(request.pageSize * (request.page - 1)).Take(request.pageSize);
            if (teamsOnThePage == null)
            {
                return BadRequest("page number is too big");
            }
            List<TeamInfo> content = new List<TeamInfo>();
            foreach (var team in teamsOnThePage)
            {
                TeamInfo info = new TeamInfo();
                info.id = team.Id;
                info.teamNumber = team.Number;
                info.projectTheme = team.Theme;
                content.Add(info);
            }
            Pagination pagination = new Pagination();
            pagination.total_elements = relevant_data.Count();
            pagination.total_pages = (relevant_data.Count() + request.pageSize - 1) / request.pageSize;
            pagination.pageSize = content.Count;
            pagination.page_number = request.page;
            TeamComplexResponse response = new TeamComplexResponse();
            response.content = content;
            response.pagination = pagination;
            return Ok(response);
        }

        [HttpPost("/admin/teams")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> CreateTeam([FromBody] TeamRequest team)
        {
            if (team.id == null)
            {
                return BadRequest("Id must be specified");
            }
            List<string> same_properties = new List<string>();
            var intersect = _context.Groups.FirstOrDefault(ob => ob.Theme == team.projectTheme);
            if (intersect != null)
            {
                same_properties.Add("projectTheme");
            }
            intersect = _context.Groups.FirstOrDefault(ob => ob.Id == team.id);
            if (intersect != null)
            {
                same_properties.Add("id");
            }

            if (same_properties.Count > 0)
            {
                return StatusCode(409, same_properties);
            }
            foreach (int id in team.members)
            {
                var user = _context.Learners.FirstOrDefault(ob => ob.Id == id);
                if (user == null)
                {
                    return NotFound("user does not exists");
                }
            }
            Group new_group = new Group();
            new_group.Id = team.id.Value;
            new_group.Number = _context.Groups.Count() + 1;
            new_group.Theme = team.projectTheme;
            new_group.Name = "";
            await _context.Groups.AddAsync(new_group);
            await _context.SaveChangesAsync();

            foreach (int id in team.members)
            {
                var user = _context.Learners.FirstOrDefault(ob => ob.Id == id);
                Relate relate = new Relate();
                relate.Learner = user;
                relate.Group = new_group;
                await _context.Relates.AddAsync(relate);
                await _context.SaveChangesAsync();
            }
            
            return Ok();
        }
        
        [HttpPut("/admin/teams")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateTeam([FromBody] TeamRequest team)
        {
            if (team.id == null)
            {
                return BadRequest("Id is not specified");
            }
            Models.Group? team_old = _context.Groups.Find(team.id);
            if (team_old == null)
            {
                return NotFound();
            }
            foreach (int id in team.members)
            {
                var user = _context.Learners.FirstOrDefault(ob => ob.Id == id);
                if (user == null)
                {
                    return NotFound("user does not exists");
                }
            }
            team_old.Theme = team.projectTheme;
            await _context.SaveChangesAsync();

            Group? team_new = await _context.Groups.FindAsync(team.id);

            var old_users = from r in _context.Relates
                             where r.GroupId == team.id
                              select r.LearnerId;
            List<int> old_users_id = old_users.ToList();
            List<int> users_out = old_users_id.Except(team.members).ToList();
            List<int> new_users = team.members.Except(old_users_id).ToList();

            foreach (int id in users_out)
            {
                Relate? relate = await _context.Relates.FindAsync(id, team.id);
                if (relate == null)
                {
                    return BadRequest("bad team members");
                }
                _context.Relates.Remove(relate);
            }
            foreach (int id in new_users)
            {
                Relate new_relate = new Relate();
                new_relate.Learner = await _context.Learners.FindAsync(id);
                new_relate.Group = team_new;
                _context.Relates.Add(new_relate);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/admin/teams/{id}")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetTeamData(int id)
        {
            Models.Group? team = await _context.Groups.FindAsync(id);
            if (team == null) { return NotFound();  }
            ExtendedTeamInfo response = new ExtendedTeamInfo();
            response.id = team.Id;
            response.teamNumber = team.Number;
            response.projectTheme = team.Theme;
            List<AccountInfo> content = new List<AccountInfo>();
            var old_users = from r in _context.Relates
                            where r.GroupId == id
                            select r.LearnerId;
            List<int> users_id = old_users.ToList();
            foreach (int user_id in users_id)
            {
                Models.Learner? acc = await _context.Learners.FindAsync(user_id);
                if (acc == null) { return NotFound(); }
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
            response.members = content;
            return Ok(response);
        }

        [HttpGet("/learner/teams/{id}")]
        public async Task<ActionResult> GetPublicTeamInfo(int id)
        {
            Models.Group? team = await _context.Groups.FindAsync(id);
            if (team == null) { return NotFound(); }
            PublicTeamInfo response = new PublicTeamInfo();
            response.id = team.Id;
            response.teamNumber = team.Number;
            response.projectTheme = team.Theme;
            List<PublicTeammateInfo> content = new List<PublicTeammateInfo>();
            var old_users = from r in _context.Relates
                            where r.GroupId == id
                            select r.LearnerId;
            List<int> users_id = old_users.ToList();
            foreach (int user_id in users_id)
            {
                Models.Learner? acc = await _context.Learners.FindAsync(user_id);
                if (acc == null) { return NotFound(); }
                PublicTeammateInfo info = new PublicTeammateInfo();
                info.email = acc.EmailLogin;
                info.role = acc.IsTracker == '0' ? "Learner" : "Tracker";
                info.id = acc.Id;
                info.fullName = acc.Surname + " " + acc.Name;
                if (acc.Messenger != null)
                {
                    info.messenger = acc.Messenger;
                }
                content.Add(info);
            }
            response.members = content;
            return Ok(response);
        }

        [HttpDelete("/admin/teams/{id}")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteTeam(int id)
        {
            Models.Group? team = await _context.Groups.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(team);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/admin/teams/select")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetTeamListByRole()
        {
            var relevant_data = from m in _context.Groups
                                select m;

            List<ShortenTeamInfo> content = new List<ShortenTeamInfo>();
            foreach (var team in relevant_data)
            {
                ShortenTeamInfo info = new ShortenTeamInfo();
                info.id = team.Id;
                info.number = team.Number;
                content.Add(info);
            }

            return Ok(content);
        }

    }
}
