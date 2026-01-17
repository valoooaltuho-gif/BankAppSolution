using Xunit;
using Moq;
using FluentAssertions;
using BankApp.Core.Models;
using BankApp.Core; // Убедись, что путь к IAccountRepository верен
using BankApp.Services;
using System;
using System.Threading.Tasks;

namespace BankApp.Tests.Services
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task TransferAsync_WhenFundsAreSufficient_ShouldUpdateBothAccounts()
        {
            // Arrange
            var mockRepo = new Mock<IAccountRepository>();

            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new CheckingAccount("Source") { Id = sourceId };
            source.Deposit(500m); // Баланс 500

            var target = new CheckingAccount("Target") { Id = targetId };
            target.Deposit(100m); // Баланс 100

            // Настройка Moq: когда сервис запросит аккаунты по ID, возвращаем наши объекты
            mockRepo.Setup(r => r.GetByIdAsync(sourceId)).ReturnsAsync(source);
            mockRepo.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync(target);

            // Настройка сохранения: метод UpdateAsync просто возвращает выполненную задачу
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);

            var service = new AccountService(mockRepo.Object, null);

            // Act
            await service.TransferAsync(sourceId, targetId, 200m);

            // Assert
            source.Balance.Should().Be(300m); // 500 - 200
            target.Balance.Should().Be(300m); // 100 + 200

            // Проверяем, что репозиторий вызывался для сохранения ОБОИХ аккаунтов
            mockRepo.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.Id == sourceId)), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.Id == targetId)), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var mockRepo = new Mock<IAccountRepository>();
            // Передаем null в качестве контекста, так как БД не нужна
            var service = new AccountService(mockRepo.Object, null);
            var accountType = "Checking";
            var ownerName = "New User";
            var initialDeposit = 100m;

            // Act
            var createdAccount = await service.CreateAccountAsync(accountType, ownerName, initialDeposit);

            // Assert
            createdAccount.Should().NotBeNull();
            createdAccount.OwnerName.Should().Be(ownerName);
            createdAccount.Balance.Should().Be(initialDeposit);
            createdAccount.Should().BeOfType<CheckingAccount>(); // Проверяем, что создан правильный тип

            // Проверяем, что метод репозитория AddAsync был вызван ровно 1 раз
            mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task DepositAsync_ShouldIncreaseBalanceAndCallUpdateAsync()
        {
            // Arrange
            var mockRepo = new Mock<IAccountRepository>();
            var service = new AccountService(mockRepo.Object, null);
            var accountId = Guid.NewGuid();
            var account = new CheckingAccount("Deposit User") { Id = accountId };
            account.Deposit(50m); // Начальный баланс 50

            // Настраиваем Mock: при GetByIdAsync вернуть наш аккаунт
            mockRepo.Setup(repo => repo.GetByIdAsync(accountId)).ReturnsAsync(account);
            mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);

            // Act
            await service.DepositAsync(accountId, 100m); // Вносим 100

            // Assert
            account.Balance.Should().Be(150m);
            // Проверяем, что UpdateAsync вызывался ровно один раз с нужным аккаунтом
            mockRepo.Verify(repo => repo.UpdateAsync(It.Is<Account>(a => a.Id == accountId)), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_WhenInsufficientFunds_ShouldThrowExceptionAndNotCallUpdate()
        {
            // Arrange
            var mockRepo = new Mock<IAccountRepository>();
            var service = new AccountService(mockRepo.Object, null);
            var accountId = Guid.NewGuid();
            var account = new CheckingAccount("Withdraw User") { Id = accountId };
            account.Deposit(100m); // Баланс 100 (доступно до 600)

            mockRepo.Setup(repo => repo.GetByIdAsync(accountId)).ReturnsAsync(account);

            // Act & Assert
            // Ожидаем ошибку InsufficientFundsException
            await service.Invoking(s => s.WithdrawAsync(accountId, 1000m))
                         .Should().ThrowAsync<InsufficientFundsException>();

            // Assert (Проверки)
            account.Balance.Should().Be(100m); // Баланс не изменился
                                               // Убеждаемся, что метод UpdateAsync НИКОГДА не вызывался при ошибке
            mockRepo.Verify(repo => repo.UpdateAsync(It.IsAny<Account>()), Times.Never);
        }


    }
}
