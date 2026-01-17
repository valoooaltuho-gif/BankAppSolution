using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankApp.Api;
using BankApp.Api.DTOs;
using BankApp.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BankApp.Tests;

public abstract class BaseIntegrationTest : IClassFixture<BankApiFactory>, IAsyncLifetime
{
    protected readonly BankApiFactory Factory;
    protected readonly HttpClient Client;
    protected IServiceScope Scope;
    protected BankContext Context;

    private SqliteConnection _connection;

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
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<BankContext>();

        // 1. Создаем и открываем соединение вручную, чтобы база не удалилась раньше времени
        _connection = new SqliteConnection($"DataSource={Factory.DatabaseName};Mode=Memory;Cache=Shared");
        await _connection.OpenAsync();

        // 2. Привязываем контекст к этому соединению
        Context.Database.SetDbConnection(_connection);

        // 3. Создаем схему и наполняем данными
        await Context.Database.MigrateAsync(); // или EnsureCreatedAsync(), если нет миграций
        await DbInitializer.SeedUsers(Context);
    }

    public virtual async Task DisposeAsync()
    {
        // 1. Удаляем базу
        if (Context != null) await Context.Database.EnsureDeletedAsync();

        // 2. Закрываем соединение (теперь база точно удалена из памяти)
        if (_connection != null) await _connection.DisposeAsync();

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
