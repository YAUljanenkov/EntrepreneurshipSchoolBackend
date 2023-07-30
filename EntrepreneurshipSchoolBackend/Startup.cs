using System.Reflection;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Task = System.Threading.Tasks.Task;

namespace EntrepreneurshipSchoolBackend;

public class Startup
{
    private String connectionString = "host=localhost;port=5432;database=database;username=admin;password=password";

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IWebHostEnvironment? env = serviceProvider.GetService<IWebHostEnvironment>();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAuthentication(options => { 
            options.DefaultScheme = "Cookies"; 
        }).AddCookie("Cookies", options => {
            options.Cookie.Name = "auth_cookie";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = env?.IsDevelopment() ?? false 
                ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            options.Events = new CookieAuthenticationEvents
            {                          
                OnRedirectToLogin = redirectContext =>
                {
                    redirectContext.HttpContext.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
            };                
        });
        services.AddAuthorization();
        // var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        services.AddDbContext<ApiDbContext>(options => { options.UseNpgsql(connectionString); });
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
        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    /// <summary>
    /// This method is used ON STARTUP to check if the DB has types (TransactionTypes, FinalGradeTypes and so on).
    /// You can also use the text of this method as a reference point for types inside the DB.
    /// </summary>
    public void StartupRecords()
    {
        var opt = new DbContextOptionsBuilder<ApiDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var cont = new ApiDbContext(opt);

        if (cont.TransactionTypes.Count() != 8)
        {
            cont.TransactionTypes.RemoveRange(cont.TransactionTypes);
            cont.TransactionTypes.AddRange(new TransactionType { Name = "Activity" },
                new TransactionType { Name = "SellLot" }, new TransactionType { Name = "AdminIncome" },
                new TransactionType { Name = "TransferIncome" }, new TransactionType { Name = "FailedDeadline" },
                new TransactionType { Name = "BuyLot" }, new TransactionType { Name = "AdminOutcome" },
                new TransactionType { Name = "TransferOutcome" });
        }

        if(cont.TaskTypes.Count() != 4)
        {
            cont.TaskTypes.RemoveRange(cont.TaskTypes);
            cont.TaskTypes.AddRange(new TaskType { Name = "HW" }, new TaskType { Name = "Test" },
                new TaskType { Name = "Competition" }, new TaskType { Name = "Exam" });
        }

        if(cont.AssessmentsTypes.Count() != 2)
        {
            cont.AssessmentsTypes.RemoveRange(cont.AssessmentsTypes);
            cont.AssessmentsTypes.AddRange(new AssessmentsType { Name = "TrackerGrade" }, new AssessmentsType { Name = "FinalGrade" });
        }

        if(cont.ClaimTypes.Count() != 4)
        {
            cont.ClaimTypes.RemoveRange(cont.ClaimTypes);
            cont.ClaimTypes.AddRange(new ClaimType { Name = "BuyingLot" }, new ClaimType { Name = "FailedDeadline" },
                new ClaimType { Name = "PlacingLot" }, new ClaimType { Name = "Transfer" });
        }

        if(cont.ClaimStatuses.Count() != 3)
        {
            cont.ClaimStatuses.RemoveRange(cont.ClaimStatuses);
            cont.ClaimStatuses.AddRange(new ClaimStatus { Name = "Waiting" }, new ClaimStatus { Name = "Approved" },
                new ClaimStatus { Name = "Declined" });
        }

        cont.SaveChanges();
    }
}