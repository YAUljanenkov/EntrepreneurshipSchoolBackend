using EntrepreneurshipSchoolBackend.Models;
using EntrepreneurshipSchoolBackend.Utility;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace EntrepreneurshipSchoolBackend;

/// <summary>
/// This class describes a job that checks deadlines and notifies learners and trackers about upcoming deadlines.
/// </summary>
public class CheckDeadlineJob : IJob
{
    /// <summary>
    /// This method is called by Quartz triggers.
    /// </summary>
    /// <param name="context"></param>
    public async Task Execute(IJobExecutionContext context)
    {
        if (!Properties.NeedSendEmail)
        {
            return;
        }
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        var opt = new DbContextOptionsBuilder<ApiDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var _context = new ApiDbContext(opt);
        List<MailboxAddress>? trackers;

        // Check for deadlines this hour.
        var tasks = await _context.Tasks.Include(x => x.Type).Where(x => x.Type.Name == "HW").Where(x =>
            x.Deadline.Date == DateTime.Today && x.Deadline.Hour == DateTime.Now.Hour).ToListAsync();
        if (tasks.Count > 0)
        {
            trackers = (await _context.Learner.Where(x => x.IsTracker == '1').ToListAsync())
                .Select(x => new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)).ToList();
            foreach (var task in tasks)
            {
                // await Mail.SendMessages(trackers, Properties.NewTaskToCheck(task.Title), Properties.NewTaskToCheckMessage(task.Title));
            }
        }

        // Check for deadlines tomorrow.
        var tomorrow = DateTime.Now.AddDays(1).ToUniversalTime();
        var tasksTomorrow = await _context.Tasks.Where(x =>
            x.Deadline.Date == tomorrow.Date && x.Deadline.Hour == tomorrow.Hour).ToListAsync();

        if (tasksTomorrow.Count > 0)
        {
            Console.WriteLine("has tomorrow.");
            var learners =  (await _context.Learner.Where(x => x.IsTracker == '0').ToListAsync())
                .Select(x => new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)).ToList();
            foreach (var task in tasksTomorrow)
            {
                await Mail.SendMessages(learners, Properties.DeadlineIsSoonTitle(task.Title),
                    Properties.DeadlineSoonMessage(task.Title));
                if (task.Lesson?.Number > 1)
                {
                    
                }
            }
        }
    }
}