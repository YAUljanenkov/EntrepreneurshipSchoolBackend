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
            .Where(x => request.lessonTitle == null || x.Title == request.lessonTitle);

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

            if (request.pageable)
            {
                if (request.page == null || request.pageSize == null)
                {
                    request.page = 1;
                    request.pageSize = 10;
                }
                var lessOnThePage = relevant_data.Skip(request.pageSize.Value * (request.page.Value - 1)).Take(request.pageSize.Value);
                if (!(await lessOnThePage.AnyAsync()))
                {
                    return BadRequest("page number is too big");
                }
                List<LessonInfo> content = new List<LessonInfo>();
                foreach (var les in lessOnThePage)
                {
                    TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                    DateTime dateTimeConverted = TimeZoneInfo.ConvertTime(les.Date, tz);
                    LessonInfo info = new LessonInfo
                    {
                        id = les.Id,
                        title = les.Title,
                        number = les.Number,
                        date = dateTimeConverted.ToString("O")
                    };
                    content.Add(info);
                }

                var dataLength = await relevant_data.CountAsync();
                Pagination pagination = new Pagination
                {
                    TotalElements = dataLength,
                    TotalPages = (dataLength + request.pageSize.Value - 1) / request.pageSize.Value,
                    PageSize = content.Count,
                    Page = request.page.Value
                };
                LessonResponse response = new LessonResponse
                {
                    content = content,
                    pagination = pagination
                };
                return Ok(response);
            } else
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                LessonResponse response = new LessonResponse
                {
                    content = await relevant_data.Select(les => new LessonInfo
                    {
                        id = les.Id,
                        title = les.Title,
                        number = les.Number,
                        date = TimeZoneInfo.ConvertTime(les.Date, tz).ToString("O")
                    }).ToListAsync()
                };
                return Ok(response);
            }
        }

        [HttpPost("/admin/lessons")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> CreateLesson([FromBody] LessonRequest lesson)
        {
            List<string> same_properties = new List<string>();
            var intersect = await _context.Lessons.FirstOrDefaultAsync(ob => ob.Number == lesson.number);
            if (intersect != null)
            {
                same_properties.Add("number");
            }
            intersect = await _context.Lessons.FirstOrDefaultAsync(ob => ob.Title == lesson.title);
            if (intersect != null)
            {
                same_properties.Add("title");
            }
            if (same_properties.Count > 0)
            {
                return StatusCode(409, same_properties);
            }

            Lesson newLesson = new Lesson
            {
                Title = lesson.title,
                Number = lesson.number,
                Date = lesson.date.ToUniversalTime(),
                Description = lesson.description
            };
            if (lesson.presLink != null)
            {
                newLesson.PresLink = lesson.presLink;
            }
            if (lesson.videoLink != null)
            {
                newLesson.VideoLink = lesson.videoLink;
            }
            _context.Lessons.Add(newLesson);
            await _context.SaveChangesAsync();
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
            Models.Lesson? les = await _context.Lessons.FindAsync(lesson.id);
            if (les == null)
            {
                return NotFound();
            }
            les.Title = lesson.title;
            les.Number = lesson.number;
            les.Date = lesson.date.ToUniversalTime();
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
            await _context.SaveChangesAsync();
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
            Models.Lesson? les = await _context.Lessons.Include(x=>x.Tasks).FirstOrDefaultAsync(x=>x.Id == id);
            if (les == null) { return NotFound(); }
            LessonExtendedInfo info = new LessonExtendedInfo();
            foreach (var task in _context.Lessons.Include(x => x.Tasks).Where(x => x.Id == id).SelectMany(x=>x.Tasks)) {
                TaskInLesson t = new TaskInLesson
                {
                    title = task.Title,
                    id = task.Id
                };
                if (task.Type.Name == "HW")
                {
                    info.homework = t;
                }
                if (task.Type.Name == "Testing") {
                    info.test = t; 
                }
            }
            info.description = les.Description;
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
            info.date = TimeZoneInfo.ConvertTime(les.Date, tz).ToString("O");
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
                ShortenLesson info = new ShortenLesson
                {
                    id = les.Id,
                    number = les.Number,
                    title = les.Title
                };
                content.Add(info);
            }

            return Ok(content);
        }

        [HttpDelete("/admin/lessons/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteLesson(int id)
        {
            Models.Lesson? les = await _context.Lessons.FindAsync(id);
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
                ShortenLessonInfo info = new ShortenLessonInfo
                {
                    id = les.Id,
                    number = les.Number
                };
                content.Add(info);
            }

            return Ok(content);
        }

        [HttpGet("/learner/lessons/{id}")]
        [Authorize]
        public async Task<ActionResult> GetLessonById(int id)
        {
            Models.Lesson? les = await _context.Lessons.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == id);
            if (les == null) { return NotFound(); }
            LessonSuperExtendedInfo info = new LessonSuperExtendedInfo();
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
            foreach (var task in _context.Lessons.Include(x => x.Tasks).Where(x => x.Id == id).SelectMany(x => x.Tasks))
            {
                ExtendedTaskInfo t = new ExtendedTaskInfo
                {
                    title = task.Title,
                    id = task.Id,
                    lesson = new ShortenLessonInfo
                    {
                        id = id,
                        number = les.Number
                    }
                };
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
                t.deadline = TimeZoneInfo.ConvertTime(task.Deadline, tz).ToString("O");
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
            info.date = TimeZoneInfo.ConvertTime(les.Date, tz).ToString("O");
            info.id = les.Id;
            info.number = les.Number;
            info.title = les.Title;
            info.presLink = les.PresLink;
            info.videoLink = les.VideoLink;
            return Ok(info);

        }
            
    }
}
