using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Services;

namespace SparkVenture
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Keep the default Identity Razor Pages UI and explicitly enable roles.
            builder.Services
                .AddDefaultIdentity<SparkVenture.Data.ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();


            builder.Services.AddScoped<IBusinessInvitationEmailSender, SmtpBusinessInvitationEmailSender>();




            var app = builder.Build();

            // The database must already have its Identity migrations applied.
            // Roles are created if they do not exist; optional bootstrap admin
            // credentials should be supplied through environment variables/secrets.
            await IdentityRoleSeeder.SeedAsync(app.Services, app.Configuration);

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            // Suggested on ALL pages (at least the invitation workflow):
            // Add after creating app and before app.MapControllerRoute, app.MapRazorPages, etc.
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                await next();
            });

            // Keep the existing Identity, DbContext, MVC, antiforgery, authentication and
            // authorization registrations/middleware as they currently are.
            // Ensure app.UseAuthentication() runs before app.UseAuthorization().
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages().WithStaticAssets();

            app.Run();
        }
    }
}
