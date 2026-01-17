using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Hosting; // Добавьте этот using

namespace BankApp.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        // Добавляем зависимость от окружения
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env; // Инициализируем окружение
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Передаем информацию об окружении в обработчик
                await HandleExceptionAsync(context, ex, _env.IsDevelopment());
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
        {
            Console.Error.WriteLine(exception.ToString());

            var code = HttpStatusCode.InternalServerError; // 500 Internal Server Error
            
            // Временно: если это окружение разработки/тестирования, 
            // возвращаем полное сообщение об ошибке, чтобы увидеть его в тестах
            var details = isDevelopment ? exception.ToString() : "An unexpected error occurred.";
            
            var result = JsonSerializer.Serialize(new { error = details });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
