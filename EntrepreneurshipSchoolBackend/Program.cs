using EntrepreneurshipSchoolBackend.Controllers;
using EntrepreneurshipSchoolBackend.Models;
using EntrepreneurshipSchoolBackend.Utility;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

namespace EntrepreneurshipSchoolBackend;
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}