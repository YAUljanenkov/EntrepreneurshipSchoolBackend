using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult> GetAssessments(int? learnerId, int? taskId, string? assessmentGrade,
            DateTime? dateFrom,
            DateTime? dateTo, string SortOrder, string sortProperty, int page, int pageSize)
        {
            // LearnerId может оказаться TaskId.
            var collection = from assessment in _context.Assessments
                select assessment;

            var dateFromValue = dateFrom?.ToUniversalTime();
            var dateToValue = dateTo?.ToUniversalTime();

            if (learnerId != null)
            {
                collection = from assessment in collection
                    where assessment.Id == learnerId
                    select assessment;
            }
            else if (taskId != null)
            {
                collection = from assessment in _context.Assessments
                    where assessment.Id == taskId
                    select assessment;
            }
            else if (assessmentGrade != null)
            {
                AssessmentsType? type = await _context.AssessmentsTypes.FindAsync(assessmentGrade);

                if (type == null)
                {
                    return NotFound();
                }

                collection = from assessment in collection
                    where _context.AssessmentsTypes.Find(assessment.AssessmentsType) == type
                    select assessment;
            }
            else if (dateFromValue != null && dateToValue != null)
            {
                collection = from assessment in collection
                    where assessment.Date < dateToValue && assessment.Date > dateFromValue
                    select assessment;
            }

            List<AssessmentDTO> content = new List<AssessmentDTO>();
            foreach (var item in collection)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                DateTime dateTimeConverted = TimeZoneInfo.ConvertTime((DateTime)item.Date, tz);
                AssessmentDTO newDto = new AssessmentDTO
                {
                    Id = item.Id,
                    issueDate = dateTimeConverted,
                    assessmentType = await _context.AssessmentsTypes.FindAsync(item.AssessmentsType)
                };

                TaskOutputDTO output = new TaskOutputDTO
                {
                    Id = item.TaskId,
                    Title = item.Task.Title
                };
                newDto.task = output;

                LearnerShortDTO learner = new LearnerShortDTO
                {
                    Id = item.LearnerId,
                    Name = item.Learner.Lastname + item.Learner.Name
                };
                newDto.learner = learner;

                newDto.assessment = item.Grade;
            }

            switch (sortProperty)
            {
                case "Name":
                    if (SortOrder == "asc")
                    {
                        content = content.OrderBy(content => content.learner.Name).ToList();
                    }
                    else if (SortOrder == "desc")
                    {
                        content = content.OrderByDescending(content => content.learner.Name).ToList();
                    }

                    break;
                case "Task":
                    if (SortOrder == "asc")
                    {
                        content = content.OrderBy(content => _context.Tasks.Find(content.task.Id)).ToList();
                    }
                    else if (SortOrder == "desc")
                    {
                        content = content.OrderByDescending(content => _context.Tasks.Find(content.task.Id)).ToList();
                    }

                    break;
                case "Date":
                    if (SortOrder == "asc")
                    {
                        content = content.OrderBy(content => content.issueDate).ToList();
                    }
                    else if (SortOrder == "desc")
                    {
                        content = content.OrderByDescending(content => content.issueDate).ToList();
                    }

                    break;
                case "Type":
                    if (SortOrder == "asc")
                    {
                        content = content.OrderBy(content => content.assessmentType.Name).ToList();
                    }
                    else if (SortOrder == "desc")
                    {
                        content = content.OrderByDescending(content => content.assessmentType.Name).ToList();
                    }

                    break;
                case "Grade":
                    if (SortOrder == "asc")
                    {
                        content = content.OrderBy(content => content.assessment).ToList();
                    }
                    else if (SortOrder == "desc")
                    {
                        content = content.OrderByDescending(content => content.assessment).ToList();
                    }

                    break;
            }

            Pagination pagination = new Pagination
            {
                Page = page,
                PageSize = pageSize,
                TotalElements = content.Count,
                TotalPages = content.Count / pageSize
            };

            AssessmentPageDTO result = new AssessmentPageDTO
            {
                pagination = pagination,
                content = content
            };

            return Ok(result);
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
            Assessments? alreadyInUse = await _context.Assessments.FindAsync(data.id);

            if (alreadyInUse != null)
            {
                return Conflict($"This assessment id is already in use: {data.id}");
            }

            // Проверим, получал ли ученик оценку за такое задание.

            alreadyInUse = await _context.Assessments.FirstOrDefaultAsync(assessment =>
                (assessment.TaskId == data.taskId && assessment.LearnerId == data.learnerId &&
                 assessment.AssessmentsType == 2));

            if (alreadyInUse != null)
            {
                return Conflict(
                    $"The learner (id = {data.learnerId})  already has an admin assessment for this task (id = {data.taskId}.");
            }

            // Если всё в порядке, создадим новый объект Assessment.
            Assessments newAssessment = new Assessments
            {
                Id = data.id
            };

            // Проверим, есть ли такой ученик и такое задание в базе данных.
            Learner? learner = await _context.Learner.FindAsync(data.learnerId);
            Models.Task? task = await _context.Tasks.FindAsync(data.taskId);

            if (task == null || learner == null)
            {
                return NotFound();
            }

            newAssessment.LearnerId = learner.Id;
            newAssessment.Learner = learner;
            newAssessment.TaskId = task.Id;
            newAssessment.Task = task;

            newAssessment.Grade = data.assessment;
            newAssessment.Date = DateTime.Now.ToUniversalTime();
            newAssessment.Comment = data.comment;
            newAssessment.AssessmentsType =
                (await _context.AssessmentsTypes.FirstOrDefaultAsync(type => type.Name == "FinalGrade")).Id;

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

            Assessments? present = await _context.Assessments.FirstOrDefaultAsync(assessment =>
                (assessment.Id == data.id && assessment.AssessmentsType == 2));

            if (present == null)
            {
                return NotFound("No admin assessment found by that id.");
            }

            present.LearnerId = data.learnerId;
            present.TaskId = data.taskId;
            present.Grade = data.assessment;
            present.Date = DateTime.Now.ToUniversalTime();
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

            Assessments? data = await _context.Assessments.FindAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            AssessmentByIdDTO result = new AssessmentByIdDTO
            {
                id = id
            };

            // Запишем информацию об ученике, получившем оценку.
            LearnerShortDTO learner = new LearnerShortDTO
            {
                Id = data.LearnerId
            };
            if (data.Learner == null)
            {
                return NotFound("Learner not found");
            }

            learner.Name = data.Learner.Surname + data.Learner.Name;
            result.learner = learner;

            // Запишем информацию об оцениваемом задании.

            TaskOutputDTO task = new TaskOutputDTO
            {
                Id = data.TaskId
            };
            if (data.Task == null)
            {
                return NotFound("Task not found");
            }

            task.Title = task.Title;
            result.task = task;

            result.AssessmentType = (await _context.AssessmentsTypes.FindAsync(data.AssessmentsType)).Name;
            // Если оценка трекерская, придётся вывести информацию о трекере.
            if (result.AssessmentType == "TrackerGrade")
            {
                LearnerShortDTO tracker = new LearnerShortDTO
                {
                    Id = data.TrackerId
                };
                if (data.Tracker == null)
                {
                    return NotFound("Tracker not found");
                }

                tracker.Name = data.Tracker.Surname + data.Tracker.Name;
                result.tracker = tracker;
            }

            result.issueDate = data.Date.ToUniversalTime();
            result.assessment = data.Grade;
            result.comment = data.Comment;

            return Ok(result);
        }

        [HttpDelete("/admin/assessments/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> DeleteAssessmentById(int id)
        {
            Assessments? present = await _context.Assessments.FindAsync(id);

            if (present == null)
            {
                return NotFound();
            }

            _context.Assessments.Remove(present);
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Метод рассчитывает оценку ученика по id на основе его результатов и текущей финальной оценки.
        /// </summary>
        /// <param name="learnerId"></param>
        /// <returns></returns>
        [HttpGet("admin/assessments/final-grades")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetFinalGrade(int learnerId)
        {
            // Проверим, есть ли такой ученик в БД.
            Learner? present = await _context.Learner.FindAsync(learnerId);

            if (present == null)
            {
                return NotFound("Learner not found");
            }

            // Высчитываем все коэффициенты оценки.

            double hw = await GetFinalGradeByType("HW", learnerId);
            double test = await GetFinalGradeByType("Testing", learnerId);
            double competition = await GetFinalGradeByType("Competitions", learnerId);
            double exam = await GetFinalGradeByType("Exams", learnerId);
            double attendance = await GetFinalAttendGrade(learnerId);

            // Высчитываем формулу.

            double hwMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "HW")).Weight;
            double testMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Testing")).Weight;
            double competitionMult =
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Competitions")).Weight;
            double examMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Exams")).Weight;
            double attendMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Attendance"))
                .Weight;

            double? bonusNullable = (await _context.Learner.FindAsync(learnerId)).GradeBonus;
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

            FinalAssessmentDTO hwDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(hw * hwMult, 2),
                type = "HW"
            };
            assessments.Add(hwDTO);

            FinalAssessmentDTO testDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(test * testMult, 2),
                type = "Testing"
            };
            assessments.Add(testDTO);

            FinalAssessmentDTO compDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(competition * competitionMult, 2),
                type = "Competitions"
            };
            assessments.Add(compDTO);

            FinalAssessmentDTO examDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(exam * examMult, 2),
                type = "Exams"
            };
            assessments.Add(examDTO);

            FinalAssessmentDTO attendDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(attendance * attendMult, 2),
                type = "Attendance"
            };
            assessments.Add(attendDTO);

            FinalGradeDTO result = new FinalGradeDTO
            {
                assessments = assessments,
                bonus = bonus,
                total = total
            };

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
        private async Task<double> GetFinalGradeByType(string typeName, int learnerId)
        {
            TaskType? taskType = await _context.TaskTypes.FirstOrDefaultAsync(type => type.Name == typeName);

            if (taskType == null)
            {
                return -2;
            }

            int typeNum = taskType.Id;

            var grades = from assessment in _context.Assessments
                where (assessment.Learner.Id == learnerId && assessment.AssessmentsType == 2 &&
                       assessment.Task.Type == _context.TaskTypes.Find(typeNum))
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
        private async Task<double> GetFinalAttendGrade(int learnerId)
        {
            double allLessonsCount = await _context.Lessons.CountAsync();

            var attendedLessons = from attendedLesson in _context.Attends
                where attendedLesson.LearnerId == learnerId
                select attendedLesson;

            double attendedLessonsCount = await attendedLessons.CountAsync();

            double answer = Math.Round((attendedLessonsCount / allLessonsCount) * 10, 2);

            return answer;
        }

        [HttpGet("/assessments/formula")]
        [Authorize]
        public async Task<ActionResult> GetFinalGradeFormula()
        {
            List<FinalWeightDTO> result = new List<FinalWeightDTO>();

            FinalGradeType hw = await _context.FinalTypes.FindAsync(1);
            FinalGradeType testing = await _context.FinalTypes.FindAsync(2);
            FinalGradeType competition = await _context.FinalTypes.FindAsync(3);
            FinalGradeType exam = await _context.FinalTypes.FindAsync(4);
            FinalGradeType attend = await _context.FinalTypes.FindAsync(5);

            FinalWeightDTO hwWeight = new FinalWeightDTO
            {
                weight = hw.Weight,
                type = hw.Name
            };
            result.Add(hwWeight);

            FinalWeightDTO testWeight = new FinalWeightDTO
            {
                weight = testing.Weight,
                type = testing.Name
            };
            result.Add(testWeight);

            FinalWeightDTO competitionWeight = new FinalWeightDTO
            {
                weight = competition.Weight,
                type = competition.Name
            };
            result.Add(competitionWeight);

            FinalWeightDTO examWeight = new FinalWeightDTO
            {
                weight = exam.Weight,
                type = exam.Name
            };
            result.Add(examWeight);

            FinalWeightDTO attendWeight = new FinalWeightDTO
            {
                weight = attend.Weight,
                type = attend.Name
            };
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
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "HW")).Weight = hw.weight;
            }

            FinalWeightDTO? test = result.FirstOrDefault(type => type.type == "Testing");

            if (test != null)
            {
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Testing")).Weight = test.weight;
            }

            FinalWeightDTO? comp = result.FirstOrDefault(type => type.type == "Competitions");

            if (comp != null)
            {
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Competitions")).Weight =
                    comp.weight;
            }

            FinalWeightDTO? exam = result.FirstOrDefault(type => type.type == "Exams");

            if (exam != null)
            {
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Exams")).Weight = exam.weight;
            }

            FinalWeightDTO? attend = result.FirstOrDefault(type => type.type == "Attendance");

            if (attend != null)
            {
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Attendance")).Weight =
                    attend.weight;
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

            Learner? present = await _context.Learner.FindAsync(data.learnerId);

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
        [HttpGet("/learner/assessments")]
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
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                DateTime dateTimeConverted = TimeZoneInfo.ConvertTime((DateTime)assess.Date, tz);
                LearnerAssessmentDTO newDTO = new LearnerAssessmentDTO
                {
                    id = assess.Id,
                    taskTitle = assess.Task.Title,
                    taskType = assess.Task.Type.Name,
                    issueDate = dateTimeConverted,
                };

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

        /// <summary>
        /// Метод, который позволяет ученику увидеть собственную финальную оценку.
        /// </summary>
        /// <returns></returns>
        [HttpGet("learner/assessments/final-grades")]
        [Authorize(Roles = Roles.Learner)]
        public async Task<ActionResult> LearnerGetFinalGrades()
        {
            // Для начала, узнаем ID ученика по данных об авторизации.
            int learnerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            // Высчитываем все коэффициенты оценки.

            double hw = await GetFinalGradeByType("HW", learnerId);
            double test = await GetFinalGradeByType("Testing", learnerId);
            double competition = await GetFinalGradeByType("Competitions", learnerId);
            double exam = await GetFinalGradeByType("Exams", learnerId);
            double attendance = await GetFinalAttendGrade(learnerId);

            // Высчитываем формулу.

            double hwMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "HW")).Weight;
            double testMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Testing")).Weight;
            double competitionMult =
                (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Competitions")).Weight;
            double examMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Exams")).Weight;
            double attendMult = (await _context.FinalTypes.FirstOrDefaultAsync(type => type.Name == "Attendance"))
                .Weight;

            double? bonusNullable = (await _context.Learner.FindAsync(learnerId)).GradeBonus;
            double bonus = 0;

            if (bonusNullable != null)
            {
                bonus = bonusNullable.Value;
            }

            double total = Math.Round(hw * hwMult + testMult * testMult + competition * competitionMult +
                                      exam * examMult + attendance * attendMult + bonus, 2);

            // Теперь, когда все значения посчитаны, нужно послать эти данные с помощью DTO.
            // Запакуем их...

            List<FinalAssessmentDTO> assessments = new List<FinalAssessmentDTO>();

            FinalAssessmentDTO hwDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(hw * hwMult, 2),
                type = "HW"
            };
            assessments.Add(hwDTO);

            FinalAssessmentDTO testDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(test * testMult, 2),
                type = "Testing"
            };
            assessments.Add(testDTO);

            FinalAssessmentDTO compDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(competition * competitionMult, 2),
                type = "Competitions"
            };
            assessments.Add(compDTO);

            FinalAssessmentDTO examDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(exam * examMult, 2),
                type = "Exams"
            };
            assessments.Add(examDTO);

            FinalAssessmentDTO attendDTO = new FinalAssessmentDTO
            {
                finalAssessment = Math.Round(attendance * attendMult, 2),
                type = "Attendance"
            };
            assessments.Add(attendDTO);

            FinalGradeDTO result = new FinalGradeDTO
            {
                assessments = assessments,
                bonus = bonus,
                total = total
            };

            // И пошлём.

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
            Learner? tracker = _context.Learner.FirstOrDefault(tracker =>
                tracker.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value));

            if (tracker == null)
            {
                return NotFound();
            }

            // Конструируем новый объект оценки по имеющимся данным.
            Assessments newAssessment = new Assessments
            {
                LearnerId = data.learnerId,
                Learner = await _context.Learner.FindAsync(data.learnerId),
                TaskId = data.taskId,
                Task = await _context.Tasks.FindAsync(data.taskId),
                Grade = data.assessment,
                Date = DateTime.Now.ToUniversalTime(),
                Comment = data.comment,
                AssessmentsType = 1,
                TrackerId = tracker.Id,
                Tracker = tracker
            };

            // Проверяем, не является ли эта оценка копией какой-то другой оценки из БД.
            Assessments? possibleCopy = await _context.Assessments.FirstOrDefaultAsync(assessment =>
                assessment.LearnerId == newAssessment.LearnerId &&
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
            Learner? tracker = await _context.Learner.FirstOrDefaultAsync(user =>
                user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value));

            if (tracker == null)
            {
                return NotFound();
            }

            // Ищем подходящую оценку.
            Assessments thisAssessment = await _context.Assessments.FirstOrDefaultAsync(assessment =>
                assessment.Id == data.id && assessment.TrackerId == tracker.Id);

            if (thisAssessment == null)
            {
                return NotFound();
            }

            // Меняем информацию об оценке.
            thisAssessment.Grade = data.assessment;
            thisAssessment.Comment = data.comment;
            thisAssessment.Date = DateTime.Now.ToUniversalTime();

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}