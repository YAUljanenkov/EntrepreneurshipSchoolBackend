using EntrepreneurshipSchoolBackend.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EntrepreneurshipSchoolBackend.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class SolutionController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public SolutionController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/solutions")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> AdminGetSolutions([Required] int taskId, int? learnerId, int? teamId,
            string sortProperty, string sortOrder, int page, int pageSize)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified");
            }

            Models.Task? thisTask = await _context.Tasks.FindAsync(taskId);

            if (thisTask == null)
            {
                return NotFound("No task with such ID");
            }

            var solutions = from solution in _context.Solutions
                where solution.TaskId == taskId
                select solution;

            if (thisTask.IsGroup == true)
            {
                if (teamId == null)
                {
                    return BadRequest("Team tasks require a team id");
                }

                solutions = from solution in solutions
                    where solution.GroupId == teamId
                    select solution;
            }
            else
            {
                if (learnerId == null)
                {
                    return BadRequest("Individual tasks require a learner id");
                }

                solutions = from solution in solutions
                    where solution.LearnerId == learnerId
                    select solution;
            }

            // Пока есть (и, наверное, другие не требуются) сортировки по имени ученика/группы и дате сдачи.
            switch (sortProperty)
            {
                case "name":
                    if (sortOrder == "asc")
                    {
                        if (thisTask.IsGroup == true)
                        {
                            solutions = solutions.OrderBy(solution => solution.Group.Name);
                        }
                        else
                        {
                            solutions = solutions.OrderBy(solution => solution.Learner.Surname + solution.Learner.Name);
                        }
                    }
                    else if (sortOrder == "desc")
                    {
                        if (thisTask.IsGroup == true)
                        {
                            solutions = solutions.OrderByDescending(solution => solution.Group.Name);
                        }
                        else
                        {
                            solutions = solutions.OrderByDescending(solution =>
                                solution.Learner.Surname + solution.Learner.Name);
                        }
                    }

                    break;
                case "date":
                    if (sortOrder == "asc")
                    {
                        solutions = solutions.OrderBy(solution => solution.CompleteDate);
                    }
                    else if (sortOrder == "desc")
                    {
                        solutions = solutions.OrderByDescending(solution => solution.CompleteDate);
                    }

                    break;
            }

            List<SolutionDTO> content = new List<SolutionDTO>();

            foreach (Solution solution in solutions)
            {
                SolutionDTO newDTO = new SolutionDTO();

                if (solution.Learner != null)
                {
                    LearnerShortDTO learner = new LearnerShortDTO
                    {
                        Id = solution.Learner.Id,
                        Name = solution.Learner.Name
                    };
                    newDTO.learner = learner;
                }

                if (solution.Group != null)
                {
                    GroupDTO group = new GroupDTO
                    {
                        Id = solution.Group.Id,
                        Number = solution.Group.Number
                    };
                    newDTO.team = group;
                }

                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                newDTO.completeDateTime = TimeZoneInfo.ConvertTime(solution.CompleteDate, tz);
                newDTO.fileId = solution.fileId;
            }

            SolutionListDTO result = new SolutionListDTO
            {
                content = content
            };

            Pagination pagination = new Pagination
            {
                Page = page,
                PageSize = pageSize,
                TotalElements = content.Count,
                TotalPages = (content.Count / pageSize) + 1
            };

            result.pagination = pagination;

            return Ok(result);
        }

        [HttpGet("/learner/solutions")]
        [Authorize(Roles = Roles.Learner)]
        public async Task<ActionResult> LearnerGetSolutions(string SortProperty, string SortOrder)
        {
            List<SolutionLearnerDTO> result = new List<SolutionLearnerDTO>();

            int learnerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            var solutions = from solution in _context.Solutions
                where solution.LearnerId == learnerId
                select solution;

            // Так как требуется выводить ВСЕ ДЗ, даже не сделанные, посмотрим что не сделано.
            // Возьмём все таски-ДЗ.
            var homeworks = from task in _context.Tasks
                where task.Type.Name == "HW"
                select task;

            foreach (var solution in solutions)
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                SolutionLearnerDTO newDTO = new SolutionLearnerDTO
                {
                    task = new TaskOutputDTO
                    {
                        Id = solution.TaskId,
                        Title = solution.Task.Title
                    },
                    completeDateTime = TimeZoneInfo.ConvertTime((DateTime)solution.CompleteDate, tz),
                    deadline = solution.Task.Deadline
                };
                result.Add(newDTO);
            }

            foreach (var homework in homeworks)
            {
                var hasSolution = await _context.Solutions.FirstOrDefaultAsync(solution => solution.Task == homework);

                // Если на какое-то ДЗ НЕ найдено решения, то нам нужно отразить это в ответе. 
                if (hasSolution == null)
                {
                    SolutionLearnerDTO newDTO = new SolutionLearnerDTO
                    {
                        task = new TaskOutputDTO
                        {
                            Id = homework.Id,
                            Title = homework.Title
                        },
                        completeDateTime = null,
                        deadline = homework.Deadline
                    };
                    result.Add(newDTO);
                }
            }

            return Ok(result);
        }

        [HttpGet("/learner/solutions/{taskId}")]
        [Authorize(Roles = Roles.Learner)]
        public async Task<ActionResult> LearnerGetSolutionById(int taskId)
        {
            int learnerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            Models.Task? thisTask = await _context.Tasks.FindAsync(taskId);

            if (thisTask == null)
            {
                return NotFound();
            }

            var thisSolution = await _context.Solutions.FirstOrDefaultAsync(solution =>
                solution.TaskId == taskId && solution.LearnerId == learnerId);

            if (thisSolution == null)
            {
                return NotFound();
            }

            // Теперь наполним поле о задании в ответе на запрос.

            LessonOutputDTO lesson = new LessonOutputDTO
            {
                Id = thisTask.Lesson.Id,
                Number = thisTask.Lesson.Number
            };
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
            TaskSolutionDTO task = new TaskSolutionDTO
            {
                Id = taskId,
                title = thisTask.Title,
                lesson = lesson,
                description = thisTask.Comment,
                criteria = thisTask.Criteria,
                isTeamwork = thisTask.IsGroup,
                link = thisTask.Link,
                deadline = TimeZoneInfo.ConvertTime(thisTask.Deadline, tz),
                taskType = thisTask.Type.Name
            };
            SolutionByIdDTO result = new SolutionByIdDTO
            {
                Id = 0,
                task = task,
                completeDateTime = thisSolution.CompleteDate,
                fileId = thisSolution.fileId
            };

            List<AssessmentTrackerDTO> trackerAssessments = new List<AssessmentTrackerDTO>();

            var foundAssessments = from assessment in _context.Assessments
                where assessment.AssessmentsType == 1 && assessment.LearnerId == learnerId
                select assessment;

            foreach (var assessment in foundAssessments)
            {
                AssessmentTrackerDTO newDTO = new AssessmentTrackerDTO
                {
                    id = assessment.Id,
                    trackerName = $"{assessment.Tracker?.Surname ?? ""} {assessment.Tracker?.Name ?? ""}",
                    assessment = assessment.Grade,
                    comment = assessment.Comment
                };

                trackerAssessments.Add(newDTO);
            }

            result.trackerAssessments = trackerAssessments;

            return Ok(result);
        }

        /// <summary>
        /// Метод просмотра трекером списка заданий.
        /// </summary>
        /// <param name="sortProperty">name, notEvaluated</param>
        /// <param name="sortOrder">asc, desc</param>
        /// <returns></returns>
        [HttpGet("/tracker/solutions/tasks")]
        [Authorize(Roles = Roles.Tracker)]
        public async Task<ActionResult> TrackerGetTask(string sortProperty, string sortOrder)
        {
            // Получаем id трекера по авторизации.
            int trackerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            // Получаем все группы, в которых состоит этот трекер. 
            var groups = from relation in _context.Relates
                where relation.LearnerId == trackerId
                select _context.Groups.First(gr => gr.Id == relation.GroupId);

            List<Learner> learners = new List<Learner>();

            foreach (var gr in groups)
            {
                var thisGroupLearners = from relate in _context.Relates
                    where relate.GroupId == ((Group?)gr).Id
                    select relate.Learner;

                learners.AddRange(thisGroupLearners);
            }

            // Получаем все ДЗ.
            var homeworks = from task in _context.Tasks
                where task.Type.Name == "HW"
                select task;

            List<TrackerTaskDTO> result = new List<TrackerTaskDTO>();

            foreach (var homework in homeworks)
            {
                int comp = 0;
                int eval = 0;

                // По идее, даже если работа групповая, оценки всё равно индивидуальные, как и объекты Solutions.

                var submittedSolutions = from solution in _context.Solutions
                    where solution.Task == homework && learners.Contains(solution.Learner)
                    select solution;

                comp += await submittedSolutions.CountAsync();

                var evaluatedSolutions = from assess in _context.Assessments
                    where assess.Task == homework && learners.Contains(assess.Learner)
                    select assess;

                eval += await evaluatedSolutions.CountAsync();


                TaskOutputDTO task = new TaskOutputDTO
                {
                    Title = homework.Title,
                    Id = homework.Id
                };

                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                TrackerTaskDTO dto = new TrackerTaskDTO
                {
                    task = task,
                    deadline = TimeZoneInfo.ConvertTime(homework.Deadline, tz),
                    submittedNumber = comp,
                    notEvaluatedNumber = comp - eval
                };
                dto.deadline.AddDays(7);


                result.Add(dto);
            }

            switch (sortProperty)
            {
                case "name":
                    if (sortOrder == "asc")
                    {
                        result = result.OrderBy(dto => dto.task.Title).ToList();
                    }

                    if (sortOrder == "desc")
                    {
                        result = result.OrderByDescending(dto => dto.task.Title).ToList();
                    }

                    break;
                case "notEvaluated":
                    if (sortOrder == "asc")
                    {
                        result = result.OrderBy(dto => dto.notEvaluatedNumber).ToList();
                    }

                    if (sortOrder == "desc")
                    {
                        result = result.OrderByDescending(dto => dto.notEvaluatedNumber).ToList();
                    }

                    break;
            }

            return Ok(result);
        }

        /// <summary>
        /// Метод возвращает список сданных (и несданных) решений на ДЗ и оценки к ним.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpGet("/tracker/solutions")]
        [Authorize(Roles = Roles.Tracker)]
        public async Task<ActionResult> TrackerGetSolutionsOnTask(int taskId)
        {
            int trackerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);
            Models.Task? thisTask = await _context.Tasks.FindAsync(taskId);

            if (thisTask == null)
            {
                return NotFound();
            }

            var groups = from relation in _context.Relates
                where relation.LearnerId == trackerId
                select _context.Groups.First(gr => gr.Id == relation.GroupId);

            List<Learner> learners = new List<Learner>();

            foreach (var gr in groups)
            {
                var thisGroupLearners = from relate in _context.Relates
                    where relate.GroupId == gr.Id
                    select relate.Learner;

                learners.AddRange(thisGroupLearners);
            }

            List<TrackerSeeSolutionDTO> result = new List<TrackerSeeSolutionDTO>();

            foreach (var learner in learners)
            {
                Solution? submittedSolution = await _context.Solutions.FirstOrDefaultAsync(solution =>
                    solution.Learner == learner && solution.TaskId == taskId);
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                // Если решения нет, то ученик его не сдал.
                if (submittedSolution == null)
                {
                    TrackerSeeSolutionDTO dto = new TrackerSeeSolutionDTO
                    {
                        id = 0,
                        Name = $"{learner.Surname} {learner.Name}",
                        completedDateTime = null,
                        assessment = null
                    };
                    result.Add(dto);
                }
                else
                {
                    Assessments? grade = _context.Assessments.FirstOrDefault(assessment =>
                        assessment.Learner == learner && assessment.TaskId == taskId &&
                        assessment.TrackerId == trackerId);

                    // Если отметки нет, то трекер её не поставил.
                    if (grade == null)
                    {
                        TrackerSeeSolutionDTO dto = new TrackerSeeSolutionDTO
                        {
                            id = submittedSolution.SolutionId,
                            Name = $"{learner.Surname} {learner.Name}",
                            completedDateTime = TimeZoneInfo.ConvertTime(submittedSolution.CompleteDate, tz),
                            assessment = null
                        };
                        result.Add(dto);
                    }
                    else
                    {
                        TrackerSeeSolutionDTO dto = new TrackerSeeSolutionDTO
                        {
                            id = submittedSolution.SolutionId,
                            Name = $"{learner.Surname} {learner.Name}",
                            completedDateTime = TimeZoneInfo.ConvertTime(submittedSolution.CompleteDate, tz),
                            assessment = grade.Grade
                        };
                        result.Add(dto);
                    }
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Метод возвращает подробную информацию о решении для трекера.
        /// </summary>
        /// <param name="solutionId"></param>
        /// <returns></returns>
        [HttpGet("/tracker/solutions/{id}")]
        [Authorize(Roles = Roles.Tracker)]
        public async Task<ActionResult> TrackerInspectSolution(int solutionId)
        {
            int trackerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            Solution? thisSolution = await _context.Solutions.FindAsync(solutionId);
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

            if (thisSolution == null)
            {
                return NotFound();
            }

            LearnerShortDTO learner = new LearnerShortDTO
            {
                Name = thisSolution.Learner.Surname + " " + thisSolution.Learner.Name,
                Id = thisSolution.LearnerId
            };

            // Ищем команду.
            Group group = (await _context.Relates.FirstAsync(relate => relate.LearnerId == learner.Id)).Group;

            ShortenTeamInfo team = new ShortenTeamInfo
            {
                id = group.Id,
                number = group.Number
            };


            // Заполняем данные о задании.
            LessonOutputDTO lesson = new LessonOutputDTO
            {
                Id = thisSolution.Task.Lesson.Id,
                Number = thisSolution.Task.Lesson.Number
            };

            TaskSolutionDTO task = new TaskSolutionDTO
            {
                Id = thisSolution.TaskId,
                title = thisSolution.Task.Title,
                deadline = TimeZoneInfo.ConvertTime(thisSolution.Task.Deadline, tz),
                description = thisSolution.Task.Comment,
                criteria = thisSolution.Task.Criteria,
                isTeamwork = thisSolution.Task.IsGroup,
                taskType = thisSolution.Task.Type.Name,
                lesson = lesson,
                link = thisSolution.Task.Link
            };

            TrackerInspectSolutionDTO dto = new TrackerInspectSolutionDTO
            {
                learner = learner,
                team = team,
                id = thisSolution.SolutionId,
                task = task,
                completeDateTime = TimeZoneInfo.ConvertTime(thisSolution.CompleteDate, tz),
                fileId = thisSolution.fileId
            };

            Assessments? assess = await _context.Assessments.FirstOrDefaultAsync(assessment =>
                assessment.TaskId == thisSolution.TaskId && assessment.LearnerId == thisSolution.LearnerId &&
                assessment.TrackerId == trackerId);

            if (assess != null)
            {
                dto.comment = assess.Comment;
                dto.assessment = assess.Grade;
            }
            else
            {
                dto.comment = null;
                dto.assessment = null;
            }

            return Ok(dto);
        }
    }
}