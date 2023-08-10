using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using EntrepreneurshipSchoolBackend.Utility;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public TaskController(ApiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Вывести список задания (в зависимости от способа сортировки и страницы)
        /// </summary>
        /// <param name="lesson_id"></param>
        /// <param name="taskType"></param>
        /// <param name="sortProperty"></param>
        /// <param name="sortOrder"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("/admin/tasks")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetTasks(int? lesson_id, string taskType, string sortProperty, string sortOrder,
            int page, int pageSize)
        {
            if (taskType != "HW" && taskType != "Competition" && taskType != "Exam" && taskType != "Test")
            {
                return BadRequest("Incorrect parameters");
            }

            if (sortOrder != "asc" && sortOrder != "desc")
            {
                return BadRequest("Incorrect parameters");
            }

            IEnumerable<Models.Task> tasks;

            if (lesson_id != null)
            {
                tasks = from task in _context.Tasks
                    where task.Lesson == _context.Lessons.Find(lesson_id)
                    select task;

                if (taskType != null)
                {
                    tasks = from task in tasks
                        where task.Type == _context.TaskTypes.FirstOrDefault(type => type.Name == taskType)
                        select task;
                }
            }
            else if (taskType != null)
            {
                tasks = from task in _context.Tasks
                    where task.Type == _context.TaskTypes.FirstOrDefault(type => type.Name == taskType)
                    select task;
            }
            else
            {
                return NotFound();
            }

            if (sortProperty != null)
            {
                switch (sortProperty)
                {
                    case "deadline":
                        if (sortOrder == "asc")
                        {
                            tasks = tasks.OrderBy(task => task.Deadline).ToList();
                        }
                        else
                        {
                            tasks = tasks.OrderByDescending(task => task.Deadline).ToList();
                        }

                        break;
                    case "link":
                        if (sortOrder == "asc")
                        {
                            tasks = tasks.OrderBy(task => task.Link).ToList();
                        }
                        else
                        {
                            tasks = tasks.OrderByDescending(task => task.Link).ToList();
                        }

                        break;
                    case "title":
                        if (sortOrder == "asc")
                        {
                            tasks = tasks.OrderBy(task => task.Title).ToList();
                        }
                        else
                        {
                            tasks = tasks.OrderByDescending(task => task.Title).ToList();
                        }

                        break;
                    case "taskType":
                        if (sortOrder == "asc")
                        {
                            tasks = tasks.OrderBy(task => task.Type.Id).ToList();
                        }
                        else
                        {
                            tasks = tasks.OrderByDescending(task => task.Type.Id).ToList();
                        }

                        break;
                    case "lesson":
                        if (sortOrder == "asc")
                        {
                            tasks = tasks.OrderBy(task => task.Lesson.Number).ToList();
                        }
                        else
                        {
                            tasks = tasks.OrderByDescending(task => task.Lesson.Number).ToList();
                        }

                        break;
                    default:
                    {
                        return NotFound();
                    }
                }
            }

            var tasksOnThePage = tasks.Skip(pageSize * (page - 1)).Take(pageSize);

            if (!tasksOnThePage.Any())
            {
                return NotFound();
            }

            List<TaskPageDTO> displayedTasks = new List<TaskPageDTO>();

            foreach (var task in tasksOnThePage)
            {
                TaskPageDTO newTask = new TaskPageDTO();

                newTask.Id = task.Id;
                newTask.Title = task.Title;
                newTask.deadline = task.Deadline;
                newTask.TaskType = task.Type.Name;

                if (task.Lesson != null)
                {
                    LessonOutputDTO lesson = new LessonOutputDTO();

                    lesson.Id = task.Lesson.Id;
                    lesson.Number = task.Lesson.Number;

                    newTask.Lesson = lesson;
                }
                else
                {
                    newTask.Lesson = null;
                }

                displayedTasks.Add(newTask);
            }

            Pagination pages = new Pagination();

            pages.Page = page;
            pages.PageSize = pageSize;
            pages.TotalElements = tasks.Count();
            pages.TotalPages = tasks.Count() / pageSize;

            var response = new TaskOutputPageDTO();
            response.pagination = pages;
            response.content = displayedTasks;

            return Ok(response);
        }


        /// <summary>
        /// Метод создания новых тасков и записи их в БД.
        /// Использует так называемый DTO (Data Transfer Object), 
        /// чтобы принимать данные с сервера именно так, как описано в API.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="link"></param>
        /// <param name="deadline"></param>
        /// <param name="title"></param>
        /// <param name="criteria"></param>
        /// <param name="comment"></param>
        /// <param name="lesson_id"></param>
        /// <param name="task_type"></param>
        /// <param name="is_group"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("/admin/tasks")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> CreateTask(TaskInputDTO data)
        {
            if (data.deadline == null || data.title == null || data.taskType == null)
            {
                return BadRequest("Some of the crucial fields were not specified!");
            }

            Models.Task newTask = new Models.Task
            {
                Title = data.title,
                Deadline = data.deadline.Value,
                IsGroup = data.isTeamWork,
                Comment = data.description,
                Criteria = data.criteria,
                Link = data.link
            };

            if (data.lessonId != null)
            {
                Lesson? connectedLesson = await _context.Lessons.FindAsync(data.lessonId);
                if (connectedLesson == null)
                {
                    return NotFound();
                }

                newTask.Lesson = connectedLesson;
            }

            TaskType? connectedType = await _context.TaskTypes.FirstOrDefaultAsync(type => type.Name == data.taskType);
            if (connectedType == null)
            {
                return NotFound();
            }

            newTask.Type = connectedType;

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            if (!Properties.NeedSendEmail) return Ok(newTask);
            var emails = (await _context.Learner.Where(x => x.IsTracker == '0').ToListAsync()).Select(x =>
                new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin));
            await Mail.SendMessages(emails, Properties.NewTaskTitle(data.title),
                Properties.NewTaskMessage(data.title, data.deadline.Value));

            return Ok(newTask);
        }

        /// <summary>
        /// Метод обновляет свойства какого-нибудь таска по Id.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("/admin/tasks")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateTask([FromBody] TaskInputDTO data)
        {
            if (data.id == null)
            {
                return BadRequest("Some of the crucial properties were not specified!");
            }

            Models.Task? updatedTask = _context.Tasks.Find(data.id);
            if (updatedTask == null)
            {
                return NotFound();
            }

            updatedTask.Title = data.title;
            updatedTask.Criteria = data.criteria;
            updatedTask.Comment = data.description;
            updatedTask.Deadline = data.deadline.Value;
            updatedTask.IsGroup = data.isTeamWork;
            updatedTask.Link = data.link;

            if (data.lessonId != null)
            {
                Lesson? connectedLesson = _context.Lessons.Find(data.lessonId);
                if (connectedLesson == null)
                {
                    return NotFound();
                }

                updatedTask.Lesson = connectedLesson;
            }

            TaskType? connectedType = _context.TaskTypes.FirstOrDefault(type => type.Name == data.taskType);
            if (connectedType == null)
            {
                return NotFound();
            }

            await _context.SaveChangesAsync();

            return Ok(updatedTask);
        }

        /// <summary>
        /// Нахождение таска по ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/admin/tasks/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetTaskById(int id)
        {
            Models.Task? thisTask = await _context.Tasks.FindAsync(id);

            if (thisTask == null)
            {
                return NotFound();
            }

            return Ok(thisTask);
        }

        ///
        [HttpDelete("/admin/tasks/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteTaskById(int id)
        {
            Models.Task? deletedTask = await _context.Tasks.FindAsync(id);

            if (deletedTask == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(deletedTask);
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Найти таски по типу задания (ДЗ, Тест, Конкурс, Экзамен)
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        [HttpGet("/admin/tasks/select")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetTasksByFilter(string taskType)
        {
            if (taskType != "HW" && taskType != "Competition" && taskType != "Test" && taskType != "Exam")
            {
                return NotFound();
            }

            var tasks = from task in _context.Tasks
                where task.Type.Name == taskType
                select task;

            List<TaskOutputDTO> result = new List<TaskOutputDTO>();

            foreach (var task in tasks)
            {
                TaskOutputDTO dto = new TaskOutputDTO();

                dto.Id = task.Id;
                dto.Title = task.Title;

                result.Add(dto);
            }

            return Ok(result);
        }

        /// <summary>
        /// Вывод дедлайна (и краткого описания таска и урока) по айди таска.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/admin/tasks/{id}/deadline")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetDeadlineById(int id)
        {
            Models.Task? thisTask = await _context.Tasks.FindAsync(id);

            if (thisTask == null)
            {
                return NotFound();
            }

            TaskDeadlineDTO result = new TaskDeadlineDTO();

            if (thisTask.Lesson != null)
            {
                LessonOutputDTO lesson = new LessonOutputDTO();

                lesson.Id = thisTask.Lesson.Id;
                lesson.Number = thisTask.Lesson.Number;

                result.lesson = lesson;
            }
            else
            {
                result.lesson = null;
            }

            TaskOutputDTO task = new TaskOutputDTO();
            task.Id = thisTask.Id;
            task.Title = thisTask.Title;

            result.task = task;

            result.deadline = thisTask.Deadline;

            return Ok(result);
        }
    }
}