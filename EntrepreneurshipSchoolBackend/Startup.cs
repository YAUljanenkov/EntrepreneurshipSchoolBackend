using System.Reflection;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

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
        services.AddDbContext<ApiDbContext>(options =>
            options.UseNpgsql(connectionString)
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
}