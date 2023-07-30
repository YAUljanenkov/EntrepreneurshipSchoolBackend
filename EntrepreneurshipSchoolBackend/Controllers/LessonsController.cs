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
        public async Task<ActionResult> GetLessons([FromBody] LessonComplexRequest request)
        {
            var relevant_data = _context.Lessons
            .Include(x => x.Tasks)
            .Where(x => request.lessonNumber == null || x.Number.Equals(request.lessonNumber))
            .Where(x => request.lessonTitle == null || x.Title == request.lessonTitle)
            .ToList();
            
            if (request.sortProperty != null)
            {
                if (request.sortOrder == "desc")
                {
                    switch (request.sortProperty)
                    {
                        case "lessonNumber":
                            relevant_data.OrderByDescending(x=>x.Number);
                            break;
                        case "lessonTitle":
                            relevant_data.OrderByDescending(x => x.Title);
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
                        case "lessonNumber":
                            relevant_data.OrderBy(x => x.Number);
                            break;
                        case "lessonTitle":
                            relevant_data.OrderBy(x => x.Title);
                            break;
                        case "id":
                            relevant_data.OrderBy(x => x.Id);
                            break;
                        default:
                            return BadRequest("Invalid sortby parametr");

                    }
                }
            }
            if (request.pageable)
            {
                var lessOnThePage = relevant_data.Skip(request.pageSize * (request.page - 1)).Take(request.pageSize);
                if (lessOnThePage == null)
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
                pagination.total_elements = relevant_data.Count();
                pagination.total_pages = (relevant_data.Count() + request.pageSize - 1) / request.pageSize;
                pagination.pageSize = content.Count;
                pagination.page_number = request.page;
                LessonResponse response = new LessonResponse();
                response.content = content;
                response.pagination = pagination;
                return Ok(response);
            } else
            {
                List<LessonInfo> content = new List<LessonInfo>();
                foreach (var les in relevant_data)
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
            intersect = _context.Lessons.FirstOrDefault(ob => ob.Id == lesson.id);
            if (intersect != null)
            {
                same_properties.Add("id");
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
            newLesson.Id = lesson.id;
            newLesson.Title = lesson.title;
            newLesson.Number = lesson.number;
            newLesson.Date = date;
            newLesson.Description = lesson.description;
            newLesson.PresLink = lesson.presLink;
            newLesson.VideoLink = lesson.videoLink;
            _context.Lessons.Add(newLesson);
            _context.SaveChanges();
            if (lesson.homeworkId != null)
            {
                Models.Task? homework = await _context.Tasks.FindAsync(lesson.homeworkId);
                if (homework != null)
                {
                    homework.Lesson = newLesson;
                }
            }
            if (lesson.testId != null)
            {
                Models.Task? test = await _context.Tasks.FindAsync(lesson.testId);
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

            les.Id = lesson.id;
            les.Title = lesson.title;
            les.Number = lesson.number;
            les.Date = date;
            les.Description = lesson.description;
            les.PresLink = lesson.presLink;
            les.VideoLink = lesson.videoLink;
            _context.Lessons.Add(les);
            _context.SaveChanges();
            if (lesson.homeworkId != null)
            {
                Models.Task? homework = await _context.Tasks.FindAsync(lesson.homeworkId);
                if (homework != null)
                {
                    homework.Lesson = les;
                }
            }
            if (lesson.testId != null)
            {
                Models.Task? test = await _context.Tasks.FindAsync(lesson.testId);
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
                if (task.Type.Name == "homework")
                {
                    info.homework = t;
                }
                if (task.Type.Name == "test") {
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
                t.description = task.Comment;
                t.criteria = task.Criteria;
                t.isTeamWork = task.Equals(true);
                t.link = task.Link;
                t.deadline = task.Deadline.ToString();
                if (task.Type.Name == "homework")
                {
                    t.taskType = "homework";
                    info.homework = t;
                }
                if (task.Type.Name == "test")
                {
                    t.taskType = "test";
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
