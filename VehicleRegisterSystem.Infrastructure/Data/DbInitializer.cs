using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1️⃣ Seed Roles
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => r.ToString());

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2️⃣ Seed Users
            foreach (var role in Enum.GetValues(typeof(UserRole)).Cast<UserRole>())
            {
                var email = $"{role.ToString().ToLower()}@system.com";
                var existingUser = await userManager.FindByEmailAsync(email);

                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = $"{role} User",
                        Role = (UserRole)role
                    };

                    var result = await userManager.CreateAsync(user, "Password@123"); // default password
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role.ToString());
                    }
                    else
                    {
                        throw new Exception("Failed to create user for role " + role +
                            ": " + string.Join(", ", result.Errors));
                    }
                }
            }
        }
    }
}
