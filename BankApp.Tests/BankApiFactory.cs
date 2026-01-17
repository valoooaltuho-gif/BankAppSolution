using BankApp.Api;
using BankApp.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // Убедитесь, что этот using есть
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Tests;

public class BankApiFactory : WebApplicationFactory<Program>
{

    private readonly string _databaseName = Guid.NewGuid().ToString();

     public string DatabaseName => _databaseName;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Настройки JWT (без изменений)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretLongKeyForTesting12345678901234567890!",
                ["Jwt:Issuer"] = "BankApp",
                ["Jwt:Audience"] = "BankAppUsers"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient();
            // 2. Агрессивное удаление всех существующих регистраций DbContext
            var dbContextDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<BankContext>)).ToList();
            foreach (var descriptor in dbContextDescriptors) services.Remove(descriptor);

            var contextConfigurationDescriptors = services.Where(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<BankContext>)).ToList();
            foreach (var descriptor in contextConfigurationDescriptors) services.Remove(descriptor);

            // 3. Добавление ЗАМЕНЫ на InMemory с ПОСТОЯННЫМ именем
            services.AddDbContext<BankContext>(options =>
            {
                 options.UseSqlite($"DataSource={_databaseName};Mode=Memory;Cache=Shared"); 
                
            });


            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                System.Diagnostics.Debug.WriteLine("=== JWT AUTH FAILED ===");
                System.Diagnostics.Debug.WriteLine(context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                System.Diagnostics.Debug.WriteLine("=== JWT AUTH SUCCESS: Token Validated ===");
                return Task.CompletedTask;
            }
        };
    });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        // Получаем доступ к конфигурации, которую мы задали выше
                        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                        var jwtKey = configuration["Jwt:Key"];
                        var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Audience"],
                            ClockSkew = TimeSpan.Zero
                        };
                    });
        });
    }
}
