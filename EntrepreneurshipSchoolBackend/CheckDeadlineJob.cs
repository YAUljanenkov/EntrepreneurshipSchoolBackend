using EntrepreneurshipSchoolBackend.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace EntrepreneurshipSchoolBackend;

public class CheckDeadlineJob: IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        var opt = new DbContextOptionsBuilder<ApiDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var cont = new ApiDbContext(opt);
        Console.WriteLine(cont.Admins.First().EmailLogin);
        return Task.FromResult(true);
    }
}
