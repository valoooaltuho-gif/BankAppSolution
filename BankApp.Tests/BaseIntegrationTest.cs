using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankApp.Api;
using BankApp.Api.DTOs;
using BankApp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BankApp.Tests;

public abstract class BaseIntegrationTest : IClassFixture<BankApiFactory>, IAsyncLifetime
{
    protected readonly BankApiFactory Factory;
    protected readonly HttpClient Client;
    protected IServiceScope Scope;
    protected BankContext Context;

    protected BaseIntegrationTest(BankApiFactory factory)
    {
        Factory = factory;
        // Создаем клиент с настройками из вашего Factory
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public virtual async Task InitializeAsync()
    {
        // Создаем Scope для каждого теста отдельно
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<BankContext>();

        // Т.к. в Factory используется Sqlite Memory Cache=Shared, 
        // база живет пока жив процесс Factory или пока мы не сделаем EnsureCreated.
        await Context.Database.EnsureCreatedAsync(); 
        await DbInitializer.SeedUsers(Context);
    }

    public virtual async Task DisposeAsync()
    {
        // Очищаем базу после каждого теста, чтобы тесты были изолированы
        await Context.Database.EnsureDeletedAsync();
        Scope?.Dispose();
        Client?.Dispose();
    }

    // --- Хелперы, чтобы не дублировать код в тестах ---

    protected async Task AuthenticateAsync()
    {
        var loginDto = new { UserName = "admin", Password = "Password123!" };
        var response = await Client.PostAsJsonAsync("/api/Auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        // Устанавливаем заголовок для всех последующих запросов этого клиента
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", result.Token);
    }

    protected async Task<Guid> CreateAccountInternalAsync(string owner, decimal initialDeposit)
    {
        var createRequest = new
        {
            AccountType = "Checking",
            OwnerName = owner,
            InitialDeposit = initialDeposit
        };
        var response = await Client.PostAsJsonAsync("/api/Accounts", createRequest);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<AccountResponseDto>();
        return created.Id;
    }

    protected async Task<Guid> CreateAndGetAccountIdAsync()
    {
        await AuthenticateAsync();

        var createRequest = new
        {
            AccountType = "Checking",
            OwnerName = "Test Deposit User",
            InitialDeposit = 100.00m
        };

        var response = await Client.PostAsJsonAsync("/api/Accounts", createRequest);
        response.EnsureSuccessStatusCode(); // Убедимся, что создание прошло успешно

        var createdAccount = await response.Content.ReadFromJsonAsync<AccountResponseDto>();
        return createdAccount.Id;
    }
}
