using EmployeeManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuestPDF.Infrastructure;

namespace EmployeeManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           

            QuestPDF.Settings.License = LicenseType.Community;

            // ==========================================
            // MVC
            // ==========================================

            builder.Services.AddControllersWithViews();


            // ==========================================
            // AUTHENTICATION
            // ==========================================

            builder.Services
                .AddAuthentication(
                    CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });


            // ==========================================
            // DATABASE
            // ==========================================

            builder.Services.AddDbContext<EmployeeDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration
                        .GetConnectionString("DefaultConnection")));


            // ==========================================
            // SERVICES
            // ==========================================

            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            builder.Services.AddScoped<IDepartmentService, DepartmentService>();

            builder.Services.AddScoped<ILeaveService, LeaveService>();

            builder.Services.AddScoped<
                INotificationService,
                NotificationService>();


            var app = builder.Build();


            // ==========================================
            // ERROR HANDLING
            // ==========================================

            if (!app.Environment.IsDevelopment())
            {
                // 500 Server Error
                app.UseExceptionHandler(
                    "/Error/ServerError");

                app.UseHsts();
            }
            else
            {
                // Development error page
                app.UseDeveloperExceptionPage();
            }


            // ==========================================
            // HTTPS
            // ==========================================

            app.UseHttpsRedirection();


            // ==========================================
            // STATIC FILES
            // ==========================================

            app.UseStaticFiles();


            // ==========================================
            // ROUTING
            // ==========================================

            app.UseRouting();


            // ==========================================
            // AUTHENTICATION
            // ==========================================

            app.UseAuthentication();


            // ==========================================
            // AUTHORIZATION
            // ==========================================

            app.UseAuthorization();


            // ==========================================
            // STATUS CODE ERROR HANDLING
            // ==========================================

            app.UseStatusCodePagesWithReExecute(
                "/Error/{0}");


            // ==========================================
            // DEFAULT ROUTE
            // ==========================================

            app.MapControllerRoute(
                name: "default",
                pattern:
                    "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }
    }
}