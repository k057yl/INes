using INest.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace INest.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            const string adminRole = "inest_admin";
            const string adminEmail = "romank057yl@gmail.com";
            const string adminPassword = "Qwe_123";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(
                    new IdentityRole<Guid>(adminRole));
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (!result.Succeeded)
                    throw new Exception(string.Join(
                        ", ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                await userManager.AddToRoleAsync(admin, adminRole);
            }
        }
    }
}