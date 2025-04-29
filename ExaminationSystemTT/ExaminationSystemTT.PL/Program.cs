using System.Threading.Tasks;
using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.BLL.Repositories;
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExaminationSystemTT.PL
{
    public class Program
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roleNames = { "Admin", "Instructor", "Student" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles and seed them to the database
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        // Log errors
                        foreach (var error in roleResult.Errors)
                        {
                            logger.LogError($"Error creating role '{roleName}': {error.Description}");
                        }
                    }
                }
            }
        }
        // Keep SeedRolesAsync as it is

        // Function specifically for seeding the Admin User
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            // Check if Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                logger.LogError("Admin role does not exist. Cannot seed Admin user.");
                return;
            }

            // Check if admin user already exists
            var adminUser = await userManager.FindByEmailAsync("admin@yourapp.com");
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin@yourapp.com", // Use email for username generally
                    Email = "admin@yourapp.com",
                    FName = "Admin",
                    LName = "User",
                    EmailConfirmed = true,
                    IsAgree = true
                };

                var createResult = await userManager.CreateAsync(newAdminUser, "Pa$$w0rd"); // CHANGE THIS PASSWORD!

                if (createResult.Succeeded)
                {
                    logger.LogInformation("Admin user created successfully.");
                    var roleResult = await userManager.AddToRoleAsync(newAdminUser, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors) { logger.LogError($"Error adding Admin user to Admin role: {error.Description}"); }
                    }
                    else
                    {
                        logger.LogInformation("Admin user added to Admin role successfully.");
                    }
                }
                else
                {
                    foreach (var error in createResult.Errors) { logger.LogError($"Error creating Admin user: {error.Description}"); }
                }
            }
            // else { logger.LogInformation("Admin user already exists."); } // Optional log
        }

        // --- NEW Function specifically for seeding the Instructor User ---
        public static async Task SeedInstructorUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            // Check if Instructor role exists
            if (!await roleManager.RoleExistsAsync("Instructor"))
            {
                logger.LogError("Instructor role does not exist. Cannot seed Instructor user.");
                return;
            }

            // Check if instructor user already exists
            var instructorUser = await userManager.FindByEmailAsync("khaledgamal@gmail.com");
            if (instructorUser == null)
            {
                var newInstructorUser = new ApplicationUser
                {
                    // Consider consistency: Maybe username should match email?
                    UserName = "khaledgamal@gmail.com", // Using email is usually better for uniqueness
                    Email = "khaledgamal@gmail.com",
                    FName = "khaled",
                    LName = "Gamal",
                    EmailConfirmed = true, // Assume confirmed for seeded user
                    IsAgree = true
                };

                var createResult = await userManager.CreateAsync(newInstructorUser, "Pa$$w0rd"); // CHANGE THIS PASSWORD!

                if (createResult.Succeeded)
                {
                    logger.LogInformation("Instructor user 'khaled Gamal' created successfully.");
                    var roleResult = await userManager.AddToRoleAsync(newInstructorUser, "Instructor");
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors) { logger.LogError($"Error adding Instructor user to Instructor role: {error.Description}"); }
                    }
                    else
                    {
                        logger.LogInformation("Instructor user added to Instructor role successfully.");
                    }
                }
                else
                {
                    // Corrected Error Logging Variable Here!
                    foreach (var error in createResult.Errors) { logger.LogError($"Error creating Instructor user 'khaled Gamal': {error.Description}"); }
                }
            }
            // else { logger.LogInformation("Instructor user 'khaled Gamal' already exists."); } // Optional log
        }




        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            builder.Services.AddDbContext<ExaminationContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddScoped<IInstructorRepository, InsrtuctorRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IExamRepository, ExamRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IStudentAnswerRepository, StudentAnswerRepository>();
            builder.Services.AddScoped<IExamAttemptRepository, ExamAttemptRepository>();


            builder.Services.AddIdentity<ApplicationUser,IdentityRole>(config=>
            {
                //config.Password.RequireDigit = true;
                //config.Password.RequireUppercase = false;
                //config.Password.RequireLowercase = false;
                //config.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<ExaminationContext>().AddDefaultTokenProviders();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Seed Roles
                    await SeedRolesAsync(roleManager, loggerFactory.CreateLogger("RoleSeeding"));

                    // Seed Default Admin User
                    await SeedAdminUserAsync(userManager, roleManager, loggerFactory.CreateLogger("AdminSeeding"));

                    await SeedInstructorUserAsync(userManager, roleManager, loggerFactory.CreateLogger("InstructorSeeding"));

                    // You could potentially seed initial Instructors/Students here too if needed
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred during seeding.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=SignUp}/{id?}");

            app.Run();
        }
    }
}
