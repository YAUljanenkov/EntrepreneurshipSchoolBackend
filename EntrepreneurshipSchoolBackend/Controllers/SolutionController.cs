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
            if(!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified");
            }

            Models.Task? thisTask = _context.Tasks.Find(taskId);

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
                            solutions = solutions.OrderByDescending(solution => solution.Learner.Surname + solution.Learner.Name);
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

            foreach(Solution solution in solutions)
            {
                SolutionDTO newDTO = new SolutionDTO();

                if (solution.Learner != null)
                {
                    LearnerShortDTO learner = new LearnerShortDTO();
                    learner.Id = solution.Learner.Id;
                    learner.Name = solution.Learner.Name;
                    newDTO.learner = learner;
                }

                if (solution.Group != null)
                {
                    GroupDTO group = new GroupDTO();
                    group.Id = solution.Group.Id;
                    group.Number = solution.Group.Number;
                    newDTO.team = group;
                }

                newDTO.completeDateTime = solution.CompleteDate;
                newDTO.fileId = solution.fileId;
            }

            SolutionListDTO result = new SolutionListDTO();
            result.content = content;

            Pagination pagination = new Pagination();

            pagination.Page = page;
            pagination.PageSize = pageSize;
            pagination.TotalElements = content.Count();
            pagination.TotalPages = (content.Count()/pageSize) + 1;

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
                SolutionLearnerDTO newDTO = new SolutionLearnerDTO();

                TaskOutputDTO task = new TaskOutputDTO();
                task.Id = solution.TaskId;
                task.Title = solution.Task.Title;

                newDTO.task = task;
                newDTO.completeDateTime = solution.CompleteDate;
                newDTO.deadline = solution.Task.Deadline;
            }

            foreach (var homework in homeworks)
            {
                var hasSolution = _context.Solutions.FirstOrDefault(solution => solution.Task == homework);

                // Если на какое-то ДЗ НЕ найдено решения, то нам нужно отразить это в ответе. 
                if (hasSolution == null) 
                {
                    SolutionLearnerDTO newDTO = new SolutionLearnerDTO();

                    TaskOutputDTO task = new TaskOutputDTO();
                    task.Id = homework.Id;
                    task.Title = homework.Title;

                    newDTO.task = task;
                    newDTO.completeDateTime = null;
                    newDTO.deadline = homework.Deadline;
                }
            }

            return Ok(result);
        }

        [HttpGet("/learner/solutions/{taskId}")]
        [Authorize(Roles = Roles.Learner)]
        public async Task<ActionResult> LearnerGetSolutionById(int taskId)
        {
            int learnerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            Models.Task? thisTask = _context.Tasks.Find(taskId);
            
            if (thisTask == null)
            {
                return NotFound();
            }

            Solution? thisSolution = _context.Solutions.FirstOrDefault(solution => solution.TaskId == taskId && solution.LearnerId == learnerId);

            if (thisSolution == null)
            {
                return NotFound();
            }

            SolutionByIdDTO result = new SolutionByIdDTO();

            result.Id = 0;

            // Теперь наполним поле о задании в ответе на запрос.
            TaskSolutionDTO task = new TaskSolutionDTO();

            task.Id = taskId;
            task.title = thisTask.Title;
            
            LessonOutputDTO lesson = new LessonOutputDTO();
            lesson.Id = thisTask.Lesson.Id;
            lesson.Number = thisTask.Lesson.Number;
            task.lesson = lesson;

            task.description = thisTask.Comment;
            task.criteria = thisTask.Criteria;
            task.isTeamwork = thisTask.IsGroup;
            task.link = thisTask.Link;
            task.deadline = thisTask.Deadline;
            task.taskType = thisTask.Type.Name;

            result.task = task;

            result.completeDateTime = thisSolution.CompleteDate;
            result.fileId = thisSolution.fileId;

            List<AssessmentTrackerDTO> trackerAssessments = new List<AssessmentTrackerDTO>();

            var foundAssessments = from assessment in _context.Assessments
                                   where assessment.AssessmentsType == 1 && assessment.LearnerId == learnerId
                                   select assessment;

            foreach (var assessment in foundAssessments)
            {
                AssessmentTrackerDTO newDTO = new AssessmentTrackerDTO();

                newDTO.id = assessment.Id;
                newDTO.trackerName = assessment.Tracker.Surname;

                newDTO.trackerName += " ";
                newDTO.trackerName += assessment.Tracker.Name;

                newDTO.assessment = assessment.Grade;
                newDTO.comment = assessment.Comment;

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
        public async Task<ActionResult> TrackerGetTask (string sortProperty, string sortOrder)
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
                                        where relate.GroupId == gr.Id
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

                comp += submittedSolutions.Count();

                var evaluatedSolutions = from assess in _context.Assessments
                                         where assess.Task == homework && learners.Contains(assess.Learner)
                                         select assess;

                eval += evaluatedSolutions.Count();

                TrackerTaskDTO dto = new TrackerTaskDTO();

                TaskOutputDTO task = new TaskOutputDTO();
                task.Title = homework.Title;
                task.Id = homework.Id;
                dto.task = task;

                dto.deadline = homework.Deadline;
                dto.deadline.AddDays(7);
                dto.submittedNumber = comp;
                dto.notEvaluatedNumber = comp - eval;

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
            Models.Task? thisTask = _context.Tasks.Find(taskId);

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
                Solution? submittedSolution = _context.Solutions.FirstOrDefault(solution => solution.Learner == learner && solution.TaskId == taskId);

                // Если решения нет, то ученик его не сдал.
                if (submittedSolution == null)
                {
                    TrackerSeeSolutionDTO dto = new TrackerSeeSolutionDTO();
                    dto.id = 0;
                    dto.Name = learner.Surname;
                    dto.Name += " ";
                    dto.Name = learner.Name;
                    dto.completedDateTime = null;
                    dto.assessment = null;
                    result.Add(dto);
                } 
                else
                {
                    Assessments? grade = _context.Assessments.FirstOrDefault(assessment => assessment.Learner == learner && assessment.TaskId == taskId && assessment.TrackerId == trackerId);

                    // Если отметки нет, то трекер её не поставил.
                    if (grade == null)
                    {
                        TrackerSeeSolutionDTO dto = new TrackerSeeSolutionDTO();
                        dto.id = submittedSolution.SolutionId;
                        dto.Name = learner.Surname;
                        dto.Name += " ";
                        dto.Name = learner.Name;
                        dto.completedDateTime = submittedSolution.CompleteDate;
                        dto.assessment = null;
                        result.Add(dto);
                    } 
                    else
                    {
                        TrackerSeeSolutionDTO dto = new TrackerSeeSolutionDTO();
                        dto.id = submittedSolution.SolutionId;
                        dto.Name = learner.Surname;
                        dto.Name += " ";
                        dto.Name = learner.Name;
                        dto.completedDateTime = submittedSolution.CompleteDate;
                        dto.assessment = grade.Grade;
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

            if (thisSolution == null)
            {
                return NotFound();
            }

            TrackerInspectSolutionDTO dto = new TrackerInspectSolutionDTO();

            dto.id = thisSolution.SolutionId;

            LearnerShortDTO learner = new LearnerShortDTO();
            learner.Name = thisSolution.Learner.Surname + " " + thisSolution.Learner.Name;
            learner.Id = thisSolution.LearnerId;
            dto.learner = learner;

            // Ищем команду.
            Group group = (await _context.Relates.FirstAsync(relate => relate.LearnerId == learner.Id)).Group;

            ShortenTeamInfo team = new ShortenTeamInfo
            {
                id = group.Id,
                number = group.Number
            };
            dto.team = team;

            // Заполняем данные о задании.

            TaskSolutionDTO task = new TaskSolutionDTO
            {
                Id = thisSolution.TaskId,
                title = thisSolution.Task.Title,
                deadline = thisSolution.Task.Deadline,
                description = thisSolution.Task.Comment,
                criteria = thisSolution.Task.Criteria,
                isTeamwork = thisSolution.Task.IsGroup,
                taskType = thisSolution.Task.Type.Name
            };

            LessonOutputDTO lesson = new LessonOutputDTO
            {
                Id = thisSolution.Task.Lesson.Id,
                Number = thisSolution.Task.Lesson.Number
            };
            task.lesson = lesson;

            task.link = thisSolution.Task.Link;
            dto.task = task;

            // Заполняем всё остальное.

            dto.completeDateTime = thisSolution.CompleteDate;
            dto.fileId = thisSolution.fileId;
            
            Assessments? assess = _context.Assessments.FirstOrDefault(assessment => assessment.TaskId == thisSolution.TaskId && assessment.LearnerId == thisSolution.LearnerId && assessment.TrackerId == trackerId);

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
