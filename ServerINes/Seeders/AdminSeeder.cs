using INest.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace INest.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration config)
        {
            string[] roles = { "inest_admin", "inest_app_user" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            var adminEmail = config["AdminSeeder:Email"];
            var adminPassword = config["AdminSeeder:Password"];
            var displayName = config["AdminSeeder:DisplayName"] ?? "Admin";

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = displayName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(admin, "inest_admin"))
            {
                await userManager.AddToRoleAsync(admin, "inest_admin");
            }
        }
    }
}