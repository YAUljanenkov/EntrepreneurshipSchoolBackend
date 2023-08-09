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
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class LessonsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public LessonsController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/lessons")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetLessons([FromQuery] LessonComplexRequest request)
        {
            if (request.sortProperty != null && !new[] { "lessonnumber", "lessontitle", "id" }.Contains(
                request.sortProperty.ToLower()))
            {
                return BadRequest("Bad sort property");
            }
            var relevant_data = _context.Lessons
            .Include(x => x.Tasks)
            .Where(x => request.lessonNumber == null || x.Number.Equals(request.lessonNumber))
            .Where(x => request.lessonTitle == null || x.Title == request.lessonTitle)
            ;

            relevant_data = request.sortProperty?.ToLower() switch
            {
                "id" => request.sortOrder == "desc"
                    ? relevant_data.OrderByDescending(x => x.Id)
                    : relevant_data.OrderBy(x => x.Id),
                "lessonnumber" => request.sortOrder == "desc"
                    ? relevant_data.OrderByDescending(x => x.Number)
                    : relevant_data.OrderBy(x => x.Number),
                "lessontitle" => request.sortOrder == "desc"
                    ? relevant_data.OrderByDescending(x => x.Title)
                    : relevant_data.OrderBy(x => x.Title),
                _ => relevant_data
            };

            var relevant_data_list = relevant_data.ToList();
            if (request.pageable)
            {
                if (request.page == null || request.pageSize == null)
                {
                    request.page = 1;
                    request.pageSize = 10;
                }
                var lessOnThePage = relevant_data_list.Skip(request.pageSize.Value * (request.page.Value - 1)).Take(request.pageSize.Value);
                if (lessOnThePage.Count() == 0)
                {
                    return BadRequest("page number is too big");
                }
                List<LessonInfo> content = new List<LessonInfo>();
                foreach (var les in lessOnThePage)
                {
                    LessonInfo info = new LessonInfo();
                    info.id = les.Id;
                    info.title = les.Title;
                    info.number = les.Number;
                    info.date = les.Date.ToString();
                    content.Add(info);
                }
                Pagination pagination = new Pagination();
                pagination.total_elements = relevant_data_list.Count();
                pagination.total_pages = (relevant_data_list.Count() + request.pageSize.Value - 1) / request.pageSize.Value;
                pagination.pageSize = content.Count;
                pagination.page_number = request.page.Value;
                LessonResponse response = new LessonResponse();
                response.content = content;
                response.pagination = pagination;
                return Ok(response);
            } else
            {
                List<LessonInfo> content = new List<LessonInfo>();
                foreach (var les in relevant_data_list)
                {
                    LessonInfo info = new LessonInfo();
                    info.id = les.Id;
                    info.title = les.Title;
                    info.number = les.Number;
                    info.date = les.Date.ToString();
                    content.Add(info);
                }
                LessonResponse response = new LessonResponse();
                response.content = content;
                return Ok(response);
            }
        }

        [HttpPost("/admin/lessons")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> CreateLesson([FromBody] LessonRequest lesson)
        {
            List<string> same_properties = new List<string>();
            var intersect = _context.Lessons.FirstOrDefault(ob => ob.Number == lesson.number);
            if (intersect != null)
            {
                same_properties.Add("number");
            }
            intersect = _context.Lessons.FirstOrDefault(ob => ob.Title == lesson.title);
            if (intersect != null)
            {
                same_properties.Add("title");
            }
            if (same_properties.Count > 0)
            {
                return StatusCode(409, same_properties);
            }
            DateTime date;
            if (!DateTime.TryParse(lesson.date, out date))
            {
                return BadRequest("bad date");
            }

            Lesson newLesson = new Lesson();
            newLesson.Title = lesson.title;
            newLesson.Number = lesson.number;
            newLesson.Date = date;
            newLesson.Description = lesson.description;
            if (lesson.presLink != null)
            {
                newLesson.PresLink = lesson.presLink;
            }
            if (lesson.videoLink != null)
            {
                newLesson.VideoLink = lesson.videoLink;
            }
            _context.Lessons.Add(newLesson);
            _context.SaveChanges();
            if (lesson.homeworkId != null)
            {
                Models.Task? homework = await _context.Tasks.FindAsync(lesson.homeworkId);
                if (homework != null && homework.Type.Name != "HW")
                {
                    return BadRequest("not id of homework");
                }
                if (homework != null && homework.Lesson != null)
                {
                    return BadRequest("homework is already bounded");
                }
                if (homework != null)
                {
                    homework.Lesson = newLesson;
                }
            }
            if (lesson.testId != null)
            {
                Models.Task? test = await _context.Tasks.FindAsync(lesson.testId);
                if (test != null && test.Type.Name != "Testing")
                {
                    return BadRequest("not id of test");
                }
                if (test != null && test.Lesson != null)
                {
                    return BadRequest("test is already bounded");
                }
                if (test != null)
                {
                    test.Lesson = newLesson;
                }
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("/admin/lessons")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateLesson([FromBody] LessonRequest lesson)
        {
            if (lesson.id == null)
            {
                return BadRequest("Id is not specified");
            }
            Models.Lesson? les = _context.Lessons.Find(lesson.id);
            if (les == null)
            {
                return NotFound();
            }
            DateTime date;
            if (!DateTime.TryParse(lesson.date, out date))
            {
                return BadRequest("bad date");
            }
            les.Title = lesson.title;
            les.Number = lesson.number;
            les.Date = date;
            les.Description = lesson.description;
            if (lesson.presLink != null)
            {
                les.PresLink = lesson.presLink;
            }
            if (lesson.videoLink != null)
            {
                les.VideoLink = lesson.videoLink;
            }
            _context.Lessons.Add(les);
            _context.SaveChanges();
            if (lesson.homeworkId != null)
            {
                Models.Task? homework = await _context.Tasks.FindAsync(lesson.homeworkId);
                if (homework != null && homework.Type.Name != "HW")
                {
                    return BadRequest("not id of homework");
                }
                if (homework != null && homework.Lesson != null && homework.Lesson.Id != lesson.id)
                {
                    return BadRequest("homework is already bounded");
                }
                if (homework != null)
                {
                    homework.Lesson = les;
                }
            }
            if (lesson.testId != null)
            {
                Models.Task? test = await _context.Tasks.FindAsync(lesson.testId);
                if (test != null && test.Type.Name != "Testing")
                {
                    return BadRequest("not id of test");
                }
                if (test != null && test.Lesson != null && test.Lesson.Id != lesson.id)
                {
                    return BadRequest("homework is already bounded");
                }
                if (test != null)
                {
                    test.Lesson = les;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/admin/lessons/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetLessonPublicData(int id)
        {
            Models.Lesson? les = _context.Lessons.Include(x=>x.Tasks).FirstOrDefault(x=>x.Id == id);
            if (les == null) { return NotFound(); }
            LessonExtendedInfo info = new LessonExtendedInfo();
            foreach (var task in _context.Lessons.Include(x => x.Tasks).Where(x => x.Id == id).SelectMany(x=>x.Tasks)) {
                TaskInLesson t = new TaskInLesson();
                t.title = task.Title;
                t.id    = task.Id;
                if (task.Type.Name == "HW")
                {
                    info.homework = t;
                }
                if (task.Type.Name == "Testing") {
                    info.test = t; 
                }
            }
            info.description = les.Description;
            info.date = les.Date.ToString();
            info.id = les.Id;
            info.number = les.Number;
            info.title = les.Title;
            info.presLink = les.PresLink;
            info.videoLink = les.VideoLink;
            return Ok(info);
        }

        [HttpGet("/learner/lessons")]
        [Authorize]
        public async Task<ActionResult> GetLessonsPublicList()
        {
            var relevant_data = from m in _context.Lessons
                                select m;

            List<ShortenLesson> content = new List<ShortenLesson>();
            foreach (var les in relevant_data)
            {
                ShortenLesson info = new ShortenLesson();
                info.id = les.Id;
                info.number = les.Number;
                info.title = les.Title;
                content.Add(info);
            }

            return Ok(content);
        }

        [HttpDelete("/admin/lessons/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteLesson(int id)
        {
            Models.Lesson? les = _context.Lessons.Find(id);
            if (les == null)
            {
                return NotFound();
            }

            _context.Lessons.Remove(les);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/admin/lessons/select")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetLessonList()
        {

            var relevant_data = from m in _context.Lessons
                                select m;

            List<ShortenLessonInfo> content = new List<ShortenLessonInfo>();
            foreach (var les in relevant_data)
            {
                ShortenLessonInfo info = new ShortenLessonInfo();
                info.id = les.Id;
                info.number = les.Number;
                content.Add(info);
            }

            return Ok(content);
        }

        [HttpGet("/learner/lessons/{id}")]
        [Authorize]
        public async Task<ActionResult> GetLessonById(int id)
        {
            Models.Lesson? les = _context.Lessons.Include(x => x.Tasks).FirstOrDefault(x => x.Id == id);
            if (les == null) { return NotFound(); }
            LessonSuperExtendedInfo info = new LessonSuperExtendedInfo();
            foreach (var task in _context.Lessons.Include(x => x.Tasks).Where(x => x.Id == id).SelectMany(x => x.Tasks))
            {
                ExtendedTaskInfo t = new ExtendedTaskInfo();
                t.title = task.Title;
                t.id = task.Id;
                t.lesson = new ShortenLessonInfo();
                t.lesson.id = id;
                t.lesson.number = les.Number;
                if (task.Comment != null)
                {
                    t.description = task.Comment;
                }
                if (task.Criteria != null)
                {
                    t.criteria = task.Criteria;
                }
                t.isTeamWork = task.Equals(true);
                if (task.Link != null)
                {
                    t.link = task.Link;
                }
                t.deadline = task.Deadline.ToString();
                if (task.Type.Name == "HW")
                {
                    t.taskType = "HW";
                    info.homework = t;
                }
                if (task.Type.Name == "Testing")
                {
                    t.taskType = "Testing";
                    info.test = t;
                }
            }
            info.description = les.Description;
            info.date = les.Date.ToString();
            info.id = les.Id;
            info.number = les.Number;
            info.title = les.Title;
            info.presLink = les.PresLink;
            info.videoLink = les.VideoLink;
            return Ok(info);

        }
            
    }
}
