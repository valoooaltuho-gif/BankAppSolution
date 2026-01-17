using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using BankApp.Infrastructure; // Для доступа к ApiUser
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Добавьте этот using
using BankApp.Core.Models;
using System.Timers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace BankApp.Api
{



    public static class DbInitializer
    {

        public static async Task SeedUsers(BankContext context)
        {
            var userManager = context.GetService<UserManager<ApiUser>>();

            // Добавляем проверку на null на всякий случай
            if (userManager == null)
            {
                throw new InvalidOperationException("Не удалось получить UserManager из контекста.");
            }

            await SeedUsersInternal(userManager);
        }

        public static async Task SeedUsers(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApiUser>>();
            await SeedUsersInternal(userManager);
        }
        }

        public static async Task SeedUsersInternal(UserManager<ApiUser> userManager)
        {

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var user = new ApiUser
                {
                    UserName = "admin",
                    Email = "admin@bank.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Password123!");

                if (result.Succeeded)
                {
                    Console.WriteLine("Пользователь 'admin' успешно создан с паролем 'Password123!'.");
                }
                else
                {
                    Console.WriteLine("Ошибка при создании пользователя admin:");
                    foreach (var error in result.Errors)
                    {
                        // Это выведет в консоль конкретную причину (например, "Password too short")
                        Console.WriteLine($" - Код: {error.Code}, Описание: {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Пользователь 'admin' уже существует. Пароль не менялся.");
            }
        }
    }

}
