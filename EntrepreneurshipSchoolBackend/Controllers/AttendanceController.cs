using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class AttendanceController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public AttendanceController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/attendance")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> GetAttendancyByLesson (int id)
        {
            Lesson? thisLesson = _context.Lessons.Find(id);

            if(thisLesson == null)
            {
                return NotFound();
            }

            var learnerRecords = from attend in _context.Attends
                                 let learner = _context.Learner.Find(attend.LearnerId)
                                 where attend.LessonId == id && attend.DidCome == '1' && learner.IsTracker == '0'
                                 select attend;

            AttendanceDTO result = new AttendanceDTO();
            
            LessonAttendancyDTO lesson = new LessonAttendancyDTO();
            lesson.Id = thisLesson.Id;
            lesson.Title = thisLesson.Title;
            lesson.Number = thisLesson.Number;
            lesson.date = thisLesson.Date;

            result.lesson = lesson;

            List<LearnerAttendDTO> learners = new List<LearnerAttendDTO>();

            foreach(var attend in learnerRecords)
            {
                Learner learner = attend.Learner;

                LearnerDTO newLearner = new LearnerDTO();

                newLearner.Id = learner.Id;
                newLearner.Name = learner.Lastname + learner.Name;
                newLearner.Role = "Learner";
                newLearner.Balance = learner.Balance;
                newLearner.Email = learner.EmailLogin;

                IEnumerable<int> teamNumbers = from relate in _context.Relates
                                               where relate.LearnerId == learner.Id
                                               select relate.Group.Number;

                newLearner.TeamNumber = teamNumbers;

                LearnerAttendDTO newAttendLearner = new LearnerAttendDTO();

                newAttendLearner.learner = newLearner;
                newAttendLearner.didCome = true;
                
                if(attend.Transaction != null)
                {
                    newAttendLearner.AccruedCurrency = attend.Transaction.Sum;
                }
                else
                {
                    newAttendLearner.AccruedCurrency = 0;
                }

                learners.Add(newAttendLearner);
            }

            result.learners = learners;

            return Ok(learners);
        }
    }
}
