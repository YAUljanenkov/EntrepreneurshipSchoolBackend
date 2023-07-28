using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class AssessmentController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public AssessmentController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/assessments")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetAssessments(int? learnerId, int? taskId, string? assessmentGrade, DateTime? dateFrom,
                                                    DateTime? dateTo, string SortOrder, string sortProperty, bool pageable, int page, int pageSize)
        {
            // LearnerId может оказаться TaskId.
            var collection = from assessment in _context.Assessments
                             select assessment;

            if (learnerId != null)
            {
                collection = from assessment in collection
                             where assessment.Id == learnerId
                             select assessment;
            }
            else if (taskId != null) {
                collection = from assessment in _context.Assessments
                             where assessment.Id == taskId
                             select assessment;
            }
            else if (assessmentGrade != null)
            {
                AssessmentsType? type = _context.AssessmentsTypes.Find(assessmentGrade);

                if (type == null)
                {
                    return NotFound();
                }

                collection = from assessment in collection
                             where _context.AssessmentsTypes.Find(assessment.AssessmentsType) == type
                             select assessment;
            }
            else if (dateFrom != null && dateTo != null)
            {
                collection = from assessment in collection
                             where assessment.Date < dateTo && assessment.Date > dateFrom
                             select assessment;
            }

            List<AssessmentDTO> content = new List<AssessmentDTO>();
            foreach (var item in collection)
            {
                AssessmentDTO newDto = new AssessmentDTO();

                newDto.Id = item.Id;
                newDto.issueDate = item.Date;
                newDto.assessmentType = _context.AssessmentsTypes.Find(item.AssessmentsType);

                TaskOutputDTO output = new TaskOutputDTO();
                output.Id = item.TaskId;
                output.Title = item.Task.Title;
                newDto.task = output;

                LearnerShortDTO learner = new LearnerShortDTO();
                learner.Id = item.LearnerId;
                learner.Name = item.Learner.Lastname + item.Learner.Name;
                newDto.learner = learner;

                newDto.assessment = item.Grade;
            }

            switch (sortProperty)
            {
                case "Name":
                    if (SortOrder == "asc")
                    {
                        content.OrderBy(content => content.learner.Name);
                    }
                    else if (SortOrder == "desc")
                    {
                        content.OrderByDescending(content => content.learner.Name);
                    }
                    break;
                case "Task":
                    if (SortOrder == "asc")
                    {
                        content.OrderBy(content => _context.Tasks.Find(content.task.Id));
                    }
                    else if (SortOrder == "desc")
                    {
                        content.OrderByDescending(content => _context.Tasks.Find(content.task.Id));
                    }
                    break;
                case "Date":
                    if (SortOrder == "asc")
                    {
                        content.OrderBy(content => content.issueDate);
                    }
                    else if (SortOrder == "desc")
                    {
                        content.OrderByDescending(content => content.issueDate);
                    }
                    break;
                case "Type":
                    if (SortOrder == "asc")
                    {
                        content.OrderBy(content => content.assessmentType.Name);
                    }
                    else if (SortOrder == "desc")
                    {
                        content.OrderByDescending(content => content.assessmentType.Name);
                    }
                    break;
                case "Grade":
                    if (SortOrder == "asc")
                    {
                        content.OrderBy(content => content.assessment);
                    }
                    else if (SortOrder == "desc")
                    {
                        content.OrderByDescending(content => content.assessment);
                    }
                    break;
            }

            if (pageable)
            {
                Pagination pagination = new Pagination();

                pagination.Page = page;
                pagination.PageSize = pageSize;
                pagination.TotalElements = content.Count();
                pagination.TotalPages = content.Count() / pageSize;

                AssessmentPageDTO result = new AssessmentPageDTO();
                result.pagination = pagination;
                result.content = content;

                return Ok(result);
            }

            return Ok(content);
        }


        /// <summary>
        /// Метод создания новой оценки в БД.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("/admin/assessments")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> CreateAssessment([FromBody] AssessmentInputDTO data)
        {
            // Сначала проверим корректность ввода.
            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified.");
            }

            // Проверим, свободен ли id оценки.
            Assessments? alreadyInUse = _context.Assessments.Find(data.id);

            if (alreadyInUse != null)
            {
                return Conflict($"This assessment id is already in use: {data.id}");
            }

            // Проверим, получал ли ученик оценку за такое задание.

            alreadyInUse = _context.Assessments.FirstOrDefault(assessment => (assessment.TaskId == data.taskId && assessment.LearnerId == data.learnerId && assessment.AssessmentsType == 2));

            if (alreadyInUse != null)
            {
                return Conflict($"The learner (id = {data.learnerId})  already has an admin assessment for this task (id = {data.taskId}.");
            }

            // Если всё в порядке, создадим новый объект Assessment.
            Assessments newAssessment = new Assessments();

            newAssessment.Id = data.id;

            // Проверим, есть ли такой ученик и такое задание в базе данных.
            Learner? learner = _context.Learner.Find(data.learnerId);
            Models.Task? task = _context.Tasks.Find(data.taskId);

            if (task == null || learner == null) {
                return NotFound();
            }

            newAssessment.LearnerId = learner.Id;
            newAssessment.Learner = learner;
            newAssessment.TaskId = task.Id;
            newAssessment.Task = task;

            newAssessment.Grade = data.assessment;
            newAssessment.Date = DateTime.Now;
            newAssessment.Comment = data.comment;
            newAssessment.AssessmentsType = _context.AssessmentsTypes.FirstOrDefault(type => type.Name == "FinalGrade").Id;

            // Когда всё готово, запишем новую админскую оценку в БД.

            _context.Assessments.Add(newAssessment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("/admin/assessments")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateAssessment([FromBody] AssessmentInputDTO data)
        {
            // Проверим корректность введённых данных.
            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified");
            }

            // Проверим, что соответствующая админская оценка есть в базе.

            Assessments? present = _context.Assessments.FirstOrDefault(assessment => (assessment.Id == data.id && assessment.AssessmentsType == 2));

            if (present == null)
            {
                return NotFound("No admin assessment found by that id.");
            }

            present.LearnerId = data.learnerId;
            present.TaskId = data.taskId;
            present.Grade = data.assessment;
            present.Date = DateTime.Now;
            present.Comment = data.comment;

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Метод возврата информации об оценке по id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/admin/assessments/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetAssessmentById(int id)
        {
            // Попытаемся найти такую оценку в БД.

            Assessments? data = _context.Assessments.Find(id);

            if (data == null)
            {
                return NotFound();
            }

            AssessmentByIdDTO result = new AssessmentByIdDTO();

            result.id = id;

            // Запишем информацию об ученике, получившем оценку.
            LearnerShortDTO learner = new LearnerShortDTO();
            learner.Id = data.LearnerId;
            if (data.Learner == null)
            {
                return NotFound("Learner not found");
            }
            learner.Name = data.Learner.Surname + data.Learner.Name;
            result.learner = learner;

            // Запишем информацию об оцениваемом задании.

            TaskOutputDTO task = new TaskOutputDTO();
            task.Id = data.TaskId;
            if (data.Task == null)
            {
                return NotFound("Task not found");
            }
            task.Title = task.Title;
            result.task = task;

            result.AssessmentType = _context.AssessmentsTypes.Find(data.AssessmentsType).Name;
            // Если оценка трекерская, придётся вывести информацию о трекере.
            if (result.AssessmentType == "TrackerGrade")
            {
                LearnerShortDTO tracker = new LearnerShortDTO();
                tracker.Id = data.TrackerId;
                if (data.Tracker == null)
                {
                    return NotFound("Tracker not found");
                }
                tracker.Name = data.Tracker.Surname + data.Tracker.Name;
                result.tracker = tracker;
            }

            result.issueDate = data.Date;
            result.assessment = data.Grade;
            result.comment = data.Comment;

            return Ok(result);
        }

        [HttpDelete("/admin/assessments/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteAssessmentById(int id)
        {
            Assessments? present = _context.Assessments.Find(id);

            if (present == null)
            {
                return NotFound();
            }

            _context.Assessments.Remove(present);
            _context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Метод рассчитывает оценку ученика по id на основе его результатов и текущей финальной оценки.
        /// </summary>
        /// <param name="learnerId"></param>
        /// <returns></returns>
        [HttpGet("/assessments/final-grades")]
        [Authorize]
        public async Task<ActionResult> GetFinalGrade(int learnerId)
        {
            // Проверим, есть ли такой ученик в БД.
            Learner? present = _context.Learner.Find(learnerId);

            if (present == null)
            {
                return NotFound("Learner not found");
            }

            // Высчитываем все коэффициенты оценки.

            double hw = GetFinalGradeByType("HW", learnerId);
            double test = GetFinalGradeByType("Testing", learnerId);
            double competition = GetFinalGradeByType("Competitions", learnerId);
            double exam = GetFinalGradeByType("Exams", learnerId);
            double attendance = GetFinalAttendGrade(learnerId);

            // Высчитываем формулу.

            double hwMult = _context.FinalTypes.FirstOrDefault(type => type.Name == "HW").Weight;
            double testMult = _context.FinalTypes.FirstOrDefault(type => type.Name == "Testing").Weight;
            double competitionMult = _context.FinalTypes.FirstOrDefault(type => type.Name == "Competitions").Weight;
            double examMult = _context.FinalTypes.FirstOrDefault(type => type.Name == "Exams").Weight;
            double attendMult = _context.FinalTypes.FirstOrDefault(type => type.Name == "Attendance").Weight;

            double? bonusNullable = _context.Learner.Find(learnerId).GradeBonus;
            double bonus = 0;

            if (bonusNullable != null)
            {
                bonus = bonusNullable.Value;
            }

            double total = Math.Round(hw * hwMult + testMult * testMult + competition * competitionMult +
                exam * examMult + attendance * attendMult + bonus, 2);

            // Теперь, когда все значения посчитаны, нужно послать эти данные с помощью DTO.\
            // Запакуем их...

            List<FinalAssessmentDTO> assessments = new List<FinalAssessmentDTO>();

            FinalAssessmentDTO hwDTO = new FinalAssessmentDTO();
            hwDTO.finalAssessment = Math.Round(hw * hwMult, 2);
            hwDTO.type = "HW";
            assessments.Add(hwDTO);

            FinalAssessmentDTO testDTO = new FinalAssessmentDTO();
            testDTO.finalAssessment = Math.Round(test * testMult, 2);
            testDTO.type = "Testing";
            assessments.Add(testDTO);

            FinalAssessmentDTO compDTO = new FinalAssessmentDTO();
            compDTO.finalAssessment = Math.Round(competition * competitionMult, 2);
            compDTO.type = "Competitions";
            assessments.Add(compDTO);

            FinalAssessmentDTO examDTO = new FinalAssessmentDTO();
            examDTO.finalAssessment = Math.Round(exam * examMult, 2);
            examDTO.type = "Exams";
            assessments.Add(examDTO);

            FinalAssessmentDTO attendDTO = new FinalAssessmentDTO();
            attendDTO.finalAssessment = Math.Round(attendance * attendMult, 2);
            attendDTO.type = "Attendance";
            assessments.Add(attendDTO);

            FinalGradeDTO result = new FinalGradeDTO();
            result.assessments = assessments;
            result.bonus = bonus;
            result.total = total;

            // И пошлём.

            return Ok(result);
        }

        /// <summary>
        /// Метод выставляет финальную оценку по одному из видов контроля.
        /// Метод работает, если администратор выставил нулевые оценки тем, кто не сделал задания.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="learnerId"></param>
        /// <returns></returns>
        private double GetFinalGradeByType(string typeName, int learnerId)
        {
            TaskType? taskType = _context.TaskTypes.FirstOrDefault(type => type.Name == typeName);

            if (taskType == null)
            {
                return -2;
            }

            int typeNum = taskType.Id;

            var grades = from assessment in _context.Assessments
                         where (assessment.Learner.Id == learnerId && assessment.AssessmentsType == 2 && assessment.Task.Type == _context.TaskTypes.Find(typeNum))
                         select assessment.Grade;

            double answer = 0;
            foreach (var grade in grades)
            {
                answer += grade;
            }

            answer = Math.Round(answer / 10, 2);
            return answer;
        }

        /// <summary>
        /// Метод считает финальную оценку по посещению в процентном соотношении посещенных учеником уроков от их общего числа.
        /// </summary>
        /// <param name="learnerId"></param>
        /// <returns></returns>
        private double GetFinalAttendGrade(int learnerId)
        {
            double allLessonsCount = _context.Lessons.Count();

            var attendedLessons = from attendedLesson in _context.Attends
                                  where attendedLesson.LearnerId == learnerId
                                  select attendedLesson;

            double attendedLessonsCount = attendedLessons.Count();

            double answer = Math.Round((attendedLessonsCount / allLessonsCount) * 10, 2);

            return answer;
        }

        [HttpGet("/assessments/formula")]
        [Authorize]
        public async Task<ActionResult> GetFinalGradeFormula()
        {
            List<FinalWeightDTO> result = new List<FinalWeightDTO>();

            FinalGradeType hw = _context.FinalTypes.Find(1);
            FinalGradeType testing = _context.FinalTypes.Find(2);
            FinalGradeType competition = _context.FinalTypes.Find(3);
            FinalGradeType exam = _context.FinalTypes.Find(4);
            FinalGradeType attend = _context.FinalTypes.Find(5);

            FinalWeightDTO hwWeight = new FinalWeightDTO();
            hwWeight.weight = hw.Weight;
            hwWeight.type = hw.Name;
            result.Add(hwWeight);

            FinalWeightDTO testWeight = new FinalWeightDTO();
            testWeight.weight = testing.Weight;
            testWeight.type = testing.Name;
            result.Add(testWeight);

            FinalWeightDTO competitionWeight = new FinalWeightDTO();
            competitionWeight.weight = competition.Weight;
            competitionWeight.type = competition.Name;
            result.Add(competitionWeight);

            FinalWeightDTO examWeight = new FinalWeightDTO();
            examWeight.weight = exam.Weight;
            examWeight.type = exam.Name;
            result.Add(examWeight);

            FinalWeightDTO attendWeight = new FinalWeightDTO();
            attendWeight.weight = attend.Weight;
            attendWeight.type = attend.Name;
            result.Add(attendWeight);

            return Ok(result);
        }

        [HttpPut("/admin/assessments/formula")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateFinalFormula([FromBody] List<FinalWeightDTO> result)
        {
            FinalWeightDTO? hw = result.FirstOrDefault(type => type.type == "HW");

            if (hw != null)
            {
                _context.FinalTypes.FirstOrDefault(type => type.Name == "HW").Weight = hw.weight;
            }

            FinalWeightDTO? test = result.FirstOrDefault(type => type.type == "Testing");

            if (test != null)
            {
                _context.FinalTypes.FirstOrDefault(type => type.Name == "Testing").Weight = test.weight;
            }

            FinalWeightDTO? comp = result.FirstOrDefault(type => type.type == "Competitions");

            if (comp != null)
            {
                _context.FinalTypes.FirstOrDefault(type => type.Name == "Competitions").Weight = comp.weight;
            }

            FinalWeightDTO? exam = result.FirstOrDefault(type => type.type == "Exams");

            if (exam != null)
            {
                _context.FinalTypes.FirstOrDefault(type => type.Name == "Exams").Weight = exam.weight;
            }

            FinalWeightDTO? attend = result.FirstOrDefault(type => type.type == "Attendance");

            if (attend != null)
            {
                _context.FinalTypes.FirstOrDefault(type => type.Name == "Attendance").Weight = attend.weight;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/admin/assessments/increase-final-grade")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> IncreaseLearnerBonus([FromBody] FinalBonusDTO data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified");
            }

            Learner? present = _context.Learner.Find(data.learnerId);

            if (present == null)
            {
                return NotFound("Learner not found");
            }

            present.GradeBonus = data.bonus;

            return Ok();
        }

        /// <summary>
        /// Метод, позволяющий ученику посмотреть оценки за свои работы.
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="learnerId"></param>
        /// <returns></returns>
        [HttpGet("/learner/assessments/final")]
        [Authorize(Roles = Roles.Learner)]
        public async Task<ActionResult> LearnerGetAssessments([Required] string taskType, [Required] int learnerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Incorrect parameters");
            }

            var assessments = from assess in _context.Assessments
                              where assess.AssessmentsType == 2 && assess.Task.Type.Name == taskType && assess.LearnerId == learnerId
                              select assess;

            List<LearnerAssessmentDTO> result = new List<LearnerAssessmentDTO>();

            foreach (var assess in assessments)
            {
                LearnerAssessmentDTO newDTO = new LearnerAssessmentDTO();

                newDTO.id = assess.Id;
                newDTO.taskTitle = assess.Task.Title;
                newDTO.taskType = assess.Task.Type.Name;
                newDTO.issueDate = assess.Date;

                if (assess.Task.Lesson != null)
                {
                    newDTO.lessonId = assess.Task.Lesson.Id;
                }
                else
                {
                    newDTO.lessonId = null;
                }
                newDTO.assessment = assess.Grade;

                result.Add(newDTO);
            }

            return Ok(result);
        }

        [HttpPost("/tracker/assessments")]
        [Authorize(Roles = Roles.Tracker)]
        public async Task<ActionResult> CreateTrackerAssessment([FromBody] AssessmentInputDTO data)
        {
            // Сначала проверяем достаточность имеющихся данных.
            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified");
            }

            // С помощью авторизации мы можем найти логин трекера и найти по нему его идентификатор.
            Learner? tracker = _context.Learner.FirstOrDefault(tracker => tracker.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value));

            if (tracker == null)
            {
                return NotFound();
            }

            // Конструируем новый объект оценки по имеющимся данным.
            Assessments newAssessment = new Assessments();

            newAssessment.Id = data.id;
            newAssessment.LearnerId = data.learnerId;
            newAssessment.Learner = _context.Learner.Find(data.learnerId);
            newAssessment.TaskId = data.taskId;
            newAssessment.Task = _context.Tasks.Find(data.taskId);
            newAssessment.Grade = data.assessment;
            newAssessment.Date = DateTime.Now;
            newAssessment.Comment = data.comment;
            newAssessment.AssessmentsType = 1;
            newAssessment.TrackerId = tracker.Id;
            newAssessment.Tracker = tracker;

            // Проверяем, не является ли эта оценка копией какой-то другой оценки из БД.
            Assessments? possibleCopy = _context.Assessments.FirstOrDefault(assessment => assessment.LearnerId == newAssessment.LearnerId &&
                                                                                assessment.TaskId == newAssessment.TaskId && assessment.TrackerId == newAssessment.TrackerId);

            if (possibleCopy != null)
            {
                return Conflict("Key properties of entity already in use");
            }

            // Если всё хорошо, записываем новую оценку в БД.
            _context.Assessments.Add(newAssessment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("/tracker/assessments")]
        [Authorize(Roles = Roles.Tracker)]
        public async Task<ActionResult> UpdateTrackerAssessment([FromBody] AssessmentInputDTO data)
        {
            // Проверяем правильность введённой информации.
            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the crucial properties have not been specified");
            }

            // Ищем трекера по информации авторизации.
            Learner? tracker = _context.Learner.FirstOrDefault(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value));

            if (tracker == null)
            {
                return NotFound();
            }

            // Ищем подходящую оценку.
            Assessments thisAssessment = _context.Assessments.FirstOrDefault(assessment => assessment.Id == data.id && assessment.TrackerId == tracker.Id);

            if (thisAssessment == null)
            {
                return NotFound();
            }

            // Меняем информацию об оценке.
            thisAssessment.Grade = data.assessment;
            thisAssessment.Comment = data.comment;
            thisAssessment.Date = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
