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
public class AccountIntegrationTests : BaseIntegrationTest
{

    public AccountIntegrationTests(BankApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAccounts_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/Accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new { UserName = "admin", Password = "Password123!" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/login", loginDto);

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
        var response = await Client.PostAsJsonAsync("/api/Accounts", createRequest);

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
        var response = await Client.PostAsJsonAsync($"/api/Accounts/{accountId}/deposit", depositRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Дополнительная проверка: получаем счет снова, чтобы убедиться, что баланс обновился в БД
        var getResponse = await Client.GetAsync($"/api/Accounts/{accountId}");
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
        var response = await Client.PostAsJsonAsync($"/api/Accounts/{accountId}/withdraw", withdrawRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/api/Accounts/{accountId}");
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
        var response = await Client.PostAsJsonAsync($"/api/Accounts/{accountId}/withdraw", withdrawRequest);

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
        var response = await Client.PostAsJsonAsync($"/api/Accounts/{fromId}/transfer", transferRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var senderAcc = await (await Client.GetAsync($"/api/Accounts/{fromId}")).Content.ReadFromJsonAsync<AccountResponseDto>();
        var receiverAcc = await (await Client.GetAsync($"/api/Accounts/{toId}")).Content.ReadFromJsonAsync<AccountResponseDto>();

        senderAcc.Balance.Should().Be(700m);
        receiverAcc.Balance.Should().Be(500m);
    }

}


// Нужно для десериализации ответа в тестах
public record LoginResponse(string Token, string Message);

