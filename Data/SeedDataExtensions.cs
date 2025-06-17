using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.Models; // Namespace for ApplicationUser
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Not strictly necessary for this method, but harmless
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder; // Essential for the WebApplication extension method

namespace ElectronicsStore.Data
{
    public static class SeedDataExtensions
    {
        public static async Task SeedDataAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); // Corrected casing

                    logger.LogInformation("Attempting to seed database with Identity data.");

                    string[] roleNames = { "Admin", "Customer" };
                    foreach (var roleName in roleNames)
                    {
                        var roleExist = await roleManager.RoleExistsAsync(roleName);
                        if (!roleExist)
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleName));
                            logger.LogInformation($"Role '{roleName}' created successfully.");
                        }
                    }

                    var adminUserEmail = "admin@electronicstore.com";
                    var adminUser = await userManager.FindByEmailAsync(adminUserEmail);

                    if (adminUser == null)
                    {
                        var newAdminUser = new ApplicationUser
                        {
                            UserName = adminUserEmail,
                            Email = adminUserEmail,
                            FullName = "Administrator",
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newAdminUser, "Admin");
                            logger.LogInformation($"Admin user created and assigned to Admin role.");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                logger.LogError($"Error creating admin user: {error.Description}");
                            }
                        }
                    }

                    logger.LogInformation("Finished seeding database.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }
    }
}