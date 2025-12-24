using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using BankApp.Infrastructure; // Для доступа к ApiUser
using Microsoft.Extensions.DependencyInjection; // Добавьте этот using

namespace BankApp.Api
{
    public static class DbInitializer
    {
        public static async Task SeedUsers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApiUser>>();

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var user = new ApiUser
                {
                    UserName = "admin",
                    Email = "admin@bank.com",
                    EmailConfirmed = true
                };

                // Попробуем создать с более сложным паролем, 
                // так как "password123" часто не проходит по умолчанию.
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
