using System.Reflection;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace EntrepreneurshipSchoolBackend;

public class Startup
{
    private String connectionString = "host=localhost;port=5432;database=database;username=admin;password=password";
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        //var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        services.AddDbContext<ApiDbContext>(options =>
            options.UseNpgsql(
                connectionString
            )
        );
        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Entrepreneurship School Backend",
                    Version = "v1",
                    Description = "EntrepreneurshipSchoolBackend",
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        StartupRecords();
    }

    /// <summary>
    /// Метод, который проверяет наличие (и, при отсутствии, создаёт) записи о типах в БД.
    /// </summary>
    public void StartupRecords()
    {
        var opt = new DbContextOptionsBuilder<ApiDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var cont = new ApiDbContext(opt);

        AssessmentsType firstA = new AssessmentsType() { Name = "TrackerGrade" };
        AssessmentsType secondA = new AssessmentsType() { Name = "FinalGrade" };

        cont.CheckAndCreate(firstA, 0);
        cont.CheckAndCreate(secondA, 0);

        TaskType firstT = new TaskType() { Name = "HW" };
        TaskType secondT = new TaskType() { Name = "Test" };
        TaskType thirdT = new TaskType() { Name = "Competition" };
        TaskType fourthT = new TaskType() { Name = "Exam" };

        cont.CheckAndCreate(firstT, 0);
        cont.CheckAndCreate(secondT, 0);
        cont.CheckAndCreate(thirdT, 0);
        cont.CheckAndCreate(fourthT, 0);

        TransactionType firstTr = new TransactionType() { Name = "Activity" };
        TransactionType secondTr = new TransactionType() { Name = "SellLot" };
        TransactionType thirdTr = new TransactionType() { Name = "AdminIncome" };
        TransactionType fourTr = new TransactionType() { Name = "TranserIncome" };
        TransactionType fiveTr = new TransactionType() { Name = "FailedDeadline" };
        TransactionType sixTr = new TransactionType() { Name = "BuyLot" };
        TransactionType sevenTr = new TransactionType() { Name = "AdminOutcome" };
        TransactionType eightTr = new TransactionType() { Name = "TransferOutcome" };

        cont.CheckAndCreate(firstTr, 0);
        cont.CheckAndCreate(secondTr, 0);
        cont.CheckAndCreate(thirdTr, 0);
        cont.CheckAndCreate(fourTr, 0);
        cont.CheckAndCreate(fiveTr, 0);
        cont.CheckAndCreate(sixTr, 0);
        cont.CheckAndCreate(sevenTr, 0);
        cont.CheckAndCreate(eightTr, 0);

        ClaimType firstC = new ClaimType() { Name = "BuyingLot" };
        ClaimType secondC = new ClaimType() { Name = "FailedDeadline" };
        ClaimType thirdC = new ClaimType() { Name = "PlacingLot" };
        ClaimType fourC = new ClaimType() { Name = "Transfer" };

        cont.CheckAndCreate(firstC, 0);
        cont.CheckAndCreate(secondC, 0);
        cont.CheckAndCreate(thirdC, 0);
        cont.CheckAndCreate(fourC, 0);

        FinalGradeType firstFg = new FinalGradeType() { Name = "HW" };
        FinalGradeType secondFg = new FinalGradeType() { Name = "Testing" };
        FinalGradeType thirdFg = new FinalGradeType() { Name = "Competitions" };
        FinalGradeType fourFg = new FinalGradeType() { Name = "Exams" };
        FinalGradeType fiveFg = new FinalGradeType() { Name = "Attendance" };

        cont.CheckAndCreate(firstFg, 0.4);
        cont.CheckAndCreate(secondFg, 0.2);
        cont.CheckAndCreate(thirdFg, 0.1);
        cont.CheckAndCreate(fourFg, 0.2);
        cont.CheckAndCreate(fiveFg, 0.1);

        cont.SaveChanges();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }
        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}