using EntrepreneurshipSchoolBackend.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EntrepreneurshipSchoolBackend.DTOs;
using System.Security.Claims;

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
                            solutions.OrderBy(solution => solution.Group.Name);
                        }
                        else
                        {
                            solutions.OrderBy(solution => solution.Learner.Surname + solution.Learner.Name);
                        }
                    }
                    else if (sortOrder == "desc")
                    {
                        if (thisTask.IsGroup == true)
                        {
                            solutions.OrderByDescending(solution => solution.Group.Name);
                        }
                        else
                        {
                            solutions.OrderByDescending(solution => solution.Learner.Surname + solution.Learner.Name);
                        }
                    }

                    break;
                case "date":
                    if (sortOrder == "asc")
                    {
                        solutions.OrderBy(solution => solution.CompleteDate);
                    }
                    else if (sortOrder == "desc")
                    {
                        solutions.OrderByDescending(solution => solution.CompleteDate);
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

            // У Solution нет собственного id, но чтобы не противоречить api, пусть здесь будет 0.
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
    }
}
