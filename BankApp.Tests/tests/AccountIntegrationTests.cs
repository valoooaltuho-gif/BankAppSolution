using System.Net;
using System.Net.Http.Json;
using BankApp.Infrastructure;
using BankApp.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Xunit;
using Microsoft.EntityFrameworkCore;
using BankApp.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using BankApp.Api;
using Microsoft.Data.Sqlite;

[Collection("Sequential")]
public class AccountIntegrationTests : IClassFixture<BankApiFactory>, IAsyncLifetime
{
    private readonly BankApiFactory _factory;
    private IServiceScope _scope;
    private BankContext _context;
    private HttpClient _client;
    private SqliteConnection _connection;

    public AccountIntegrationTests(BankApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public async Task InitializeAsync()
    {
        // Создаем область видимости сервисов (scope) для ЭТОГО теста
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<BankContext>();

        _connection = new SqliteConnection($"DataSource={_factory.DatabaseName};Mode=Memory;Cache=Shared");
        await _connection.OpenAsync();

        _context.Database.SetDbConnection(_connection); 

        await _context.Database.MigrateAsync();

        await DbInitializer.SeedUsers(_context);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task GetAccounts_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/Accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new { UserName = "admin", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Token.Should().NotBeNullOrEmpty();
        result.Message.Should().NotBeNullOrEmpty(); 
    }

    [Fact]
    public async Task CreateAccount_WithValidToken_ReturnsCreatedAndPersistsData()
    {
        await AuthenticateAsync();

        var createRequest = new
        {
            AccountType = "Checking",
            OwnerName = "Test Integration User",
            InitialDeposit = 500.00m
        };

        // 2. Act - Отправка запроса на создание счета
        var response = await _client.PostAsJsonAsync("/api/Accounts", createRequest);

        // 3. Assert - Проверка заголовков и тела ответа
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdAccount = await response.Content.ReadFromJsonAsync<AccountResponseDto>();
        createdAccount.Should().NotBeNull();
        createdAccount.OwnerName.Should().Be("Test Integration User");
        createdAccount.Balance.Should().Be(500.00m);

        // Проверяем, что в заголовке Location есть ссылка на новый ресурс
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.ToString().Should().Contain(createdAccount.Id.ToString());
    }

    // В классе AccountIntegrationTests

    [Fact]
    public async Task Deposit_ToExistingAccount_IncreasesBalance()
    {
        // Arrange
        var accountId = await CreateAndGetAccountIdAsync();
        var depositAmount = 50.00m;
        var depositRequest = new { Amount = depositAmount };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/Accounts/{accountId}/deposit", depositRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Дополнительная проверка: получаем счет снова, чтобы убедиться, что баланс обновился в БД
        var getResponse = await _client.GetAsync($"/api/Accounts/{accountId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedAccount = await getResponse.Content.ReadFromJsonAsync<AccountResponseDto>();

        // Ожидаемый баланс: 100.00 (initial) + 50.00 (deposit)
        updatedAccount.Balance.Should().Be(150.00m);
    }

    [Fact]
    public async Task Withdraw_SufficientFunds_DecreasesBalance()
    {
        // Arrange
        var accountId = await CreateAndGetAccountIdAsync(); // Начальный баланс 100
        var withdrawRequest = new { Amount = 40.00m };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/Accounts/{accountId}/withdraw", withdrawRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/Accounts/{accountId}");
        var updatedAccount = await getResponse.Content.ReadFromJsonAsync<AccountResponseDto>();
        updatedAccount.Balance.Should().Be(60.00m);
    }


    [Fact]
    public async Task Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        // Arrange
        var accountId = await CreateAndGetAccountIdAsync(); // Баланс 100
        var withdrawRequest = new { Amount = 700.00m }; // Больше, чем есть

        // Act
        var response = await _client.PostAsJsonAsync($"/api/Accounts/{accountId}/withdraw", withdrawRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }



    [Fact]
    public async Task Transfer_ValidAmount_UpdatesBothBalances()
    {
        // Arrange
        await AuthenticateAsync();
        var fromId = await CreateAccountInternalAsync("Sender", 1000m);
        var toId = await CreateAccountInternalAsync("Receiver", 200m);
        var transferRequest = new { ToAccountId = toId, Amount = 300m };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/Accounts/{fromId}/transfer", transferRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var senderAcc = await (await _client.GetAsync($"/api/Accounts/{fromId}")).Content.ReadFromJsonAsync<AccountResponseDto>();
        var receiverAcc = await (await _client.GetAsync($"/api/Accounts/{toId}")).Content.ReadFromJsonAsync<AccountResponseDto>();

        senderAcc.Balance.Should().Be(700m);
        receiverAcc.Balance.Should().Be(500m);
    }











    private async Task<Guid> CreateAccountInternalAsync(string owner, decimal initialDeposit)
    {
        var createRequest = new
        {
            AccountType = "Checking",
            OwnerName = owner,
            InitialDeposit = initialDeposit
        };
        var response = await _client.PostAsJsonAsync("/api/Accounts", createRequest);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<AccountResponseDto>();
        return created.Id;
    }

    public async Task DisposeAsync()
    {
        // Очищаем БД после каждого теста
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
        }
        // Удаляем scope
        _scope?.Dispose();
        _client?.Dispose();
        await _connection.DisposeAsync();
    }

    // В классе AccountIntegrationTests
    private async Task<Guid> CreateAndGetAccountIdAsync()
    {
        await AuthenticateAsync();

        var createRequest = new
        {
            AccountType = "Checking",
            OwnerName = "Test Deposit User",
            InitialDeposit = 100.00m
        };

        var response = await _client.PostAsJsonAsync("/api/Accounts", createRequest);
        response.EnsureSuccessStatusCode(); // Убедимся, что создание прошло успешно

        var createdAccount = await response.Content.ReadFromJsonAsync<AccountResponseDto>();
        return createdAccount.Id;
    }


    private async Task AuthenticateAsync()
    {
        var loginDto = new { UserName = "admin", Password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        // ЭТО КРИТИЧЕСКИ ВАЖНО: Установка заголовка
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
    }


}


// Нужно для десериализации ответа в тестах
public record LoginResponse(string Token, string Message);

