using Microsoft.AspNetCore.Identity;
using ChainStoreSalesManagement.Models;

namespace ChainStoreSalesManagement.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            // Tạo tài khoản admin nếu chưa tồn tại
            var adminEmail = "admin@gmail.com";
            var adminPassword = "TrungHieu@2000";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                
                if (result.Succeeded)
                {
                    // Có thể thêm role admin ở đây nếu cần
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    Console.WriteLine("Error creating admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
        }
    }
}
