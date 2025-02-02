using Blazor9.Auto.Client.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blazor9.Auto.Data
{
    public class IdentitySeedData
    {
        public static void EnsurePopulated(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            CreateAdminAccountAsync(serviceProvider, configuration).Wait();
        }
        public static async Task CreateAdminAccountAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string username = configuration["Data:AdminUser:Name"] ?? "admin";
            string email = configuration["Data:AdminUser:Email"] ?? "admin@example.com";
            string password = configuration["Data:AdminUser:Password"] ?? "Secret_123";
            string role = configuration["Data:AdminUser:Role"] ?? Constants.Role_Admins;
            if (await userManager.FindByNameAsync(username) == null)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
                ApplicationUser user = new ApplicationUser
                {
                    UserName = username,
                    Email = email
                };
                IdentityResult result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
