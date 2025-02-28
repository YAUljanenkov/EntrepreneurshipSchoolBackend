﻿using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult> GetAttendancyByLesson(int id)
        {
            Lesson? thisLesson = await _context.Lessons.FindAsync(id);

            if (thisLesson == null)
            {
                return NotFound();
            }

            var learnerRecords = from attend in _context.Attends
                let learner = _context.Learner.Find(attend.LearnerId)
                where attend.LessonId == id && attend.DidCome == '1' && learner.IsTracker == '0'
                select attend;

            AttendanceDTO result = new AttendanceDTO();

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
            DateTime dateTimeConverted = TimeZoneInfo.ConvertTime(thisLesson.Date, tz);

            LessonAttendancyDTO lesson = new LessonAttendancyDTO
            {
                Id = thisLesson.Id,
                Title = thisLesson.Title,
                Number = thisLesson.Number,
                date = dateTimeConverted
            };

            result.lesson = lesson;

            List<LearnerAttendDTO> learners = new List<LearnerAttendDTO>();

            foreach (var attend in learnerRecords)
            {
                Learner learner = attend.Learner;

                LearnerDTO newLearner = new LearnerDTO
                {
                    Id = learner.Id,
                    Name = learner.Lastname + learner.Name,
                    Role = "Learner",
                    Balance = learner.Balance,
                    Email = learner.EmailLogin
                };

                IEnumerable<int> teamNumbers = from relate in _context.Relates
                    where relate.LearnerId == learner.Id
                    select relate.Group.Number;

                newLearner.TeamNumber = teamNumbers;

                LearnerAttendDTO newAttendLearner = new LearnerAttendDTO
                {
                    learner = newLearner,
                    didCome = true
                };

                if (attend.Transaction != null)
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

        [HttpPut("/admin/attendance")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> UpdateAttendancy([FromBody] UpdateAttendancyDTO data)
        {
            // Атрибут [FromBody] помогает автоматически десериализировать посланные с сервера данные в объект C#.
            // Достаём записи о посещаемости этого урока.
            var thisLessonRecords = from attend in _context.Attends
                where attend.LessonId == data.LessonId
                select attend;

            // Ищем совпадающие ID учеников и обновляем их данные.

            foreach (var record in data.learners)
            {
                Attend? attend = await _context.Attends.FindAsync(data.LessonId, record.Id);

                if (attend == null)
                {
                    return NotFound();
                }

                attend.DidCome = record.DidCome;

                // Если деньги за посещение этого урока ещё не начислялись, создаём заявку.
                if (attend.Transaction == null)
                {
                    if (record.AccruedCurrency != 0)
                    {
                        // ЕСЛИ МЫ ХРАНИМ ТРАНЗАКЦИЮ В ТАБЛИЦЕ ATTEND, ЭТУ ТРАНЗАКЦИЮ НЕЛЬЗЯ УДАЛЯТЬ!
                        // Так админ будет знать, сколько денег уже начислили ученику за посещение занятия.
                        Transaction attendTransaction = new Transaction
                        {
                            Learner = attend.Learner,
                            Comment = $"За посещение урока №{attend.Lesson}.",
                            Sum = record.AccruedCurrency,
                            // Посещение урока не требует подтверждения, а следовательно, и создания заявки.
                            Claim = null,
                            Type = await _context.TransactionTypes.FirstOrDefaultAsync(type => type.Name == "Activity"),
                            Date = DateTime.Now.ToUniversalTime()
                        };

                        _context.Transactions.Add(attendTransaction);

                        attend.Transaction = attendTransaction;
                    }
                    // Если деньги за посещения занятия ещё не начислялись и не начисляются сейчас, то транзакция не создаётся.
                }
                else
                {
                    Transaction attendTransaction = new Transaction
                    {
                        Learner = attend.Learner,
                        Comment = $"Изменение вознаграждения за посещение урока №{attend.Lesson}.",
                        // Если транзакция уже проходила за посещение этого урока, то нам нужно слегка по-другому считать деньги за новую транзакцию.
                        Sum = record.AccruedCurrency - attend.Transaction.Sum,
                        Claim = null,
                        Type = await _context.TransactionTypes.FirstOrDefaultAsync(type => type.Name == "Activity"),
                        Date = DateTime.Now.ToUniversalTime()
                    };


                    _context.Transactions.Remove(attend.Transaction);
                    _context.Transactions.Add(attendTransaction);

                    attend.Transaction = attendTransaction;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}