using Xunit;
using FluentAssertions; // Убедись, что пакет установлен: dotnet add package FluentAssertions
using BankApp.Core.Models;
using Moq;
using BankApp.Services;

namespace BankApp.Tests.Core
{
    public class AccountTests
    {
        [Fact]
        public void Withdraw_WhenBalanceIsSufficient_ShouldDecreaseBalance()
        {
            // Arrange (Подготовка)
            // Создаем аккаунт и вносим начальную сумму
            var account = new CheckingAccount("Test Owner");
            account.Deposit(500m);

            // Act (Действие)
            // Снимаем часть средств
            account.Withdraw(200m);

            // Assert (Проверка)
            // Баланс должен стать 300
            account.Balance.Should().Be(300m);
        }

        [Fact]
        public void Withdraw_WhenAmountExceedsOverdraftLimit_ShouldThrowInsufficientFundsException()
        {
            // Arrange
            var account = new CheckingAccount("Test Owner");
            account.Deposit(100m); // Баланс 100 (доступно до 600)

            // Act
            Action action = () => account.Withdraw(1000m); // Пытаемся снять 1000

            // Assert
            action.Should().Throw<InsufficientFundsException>()
                  // Проверяем точное сообщение из твоего класса CheckingAccount.cs
                  .WithMessage("Withdrawal amount exceeds overdraft limit.");

            // Баланс не должен измениться
            account.Balance.Should().Be(100m);
        }

        [Fact]
        public void SavingsAccount_Withdraw_WhenExceedsBalance_ShouldThrowInsufficientFundsException()
        {
            // Arrange
            var account = new SavingsAccount("Savings Owner");
            account.Deposit(100m);

            // Act
            Action action = () => account.Withdraw(100.01m); // Пытаемся снять чуть больше, чем есть

            // Assert
            action.Should().Throw<InsufficientFundsException>()
                  .WithMessage("Insufficient funds. Savings accounts do not allow overdrafts.");

            account.Balance.Should().Be(100m);
        }
        [Fact]
        public void SavingsAccount_ApplyInterest_ShouldCalculateExactly2Percent()
        {
            // Arrange
            var account = new SavingsAccount("Savings Owner");
            account.Deposit(1000m); // Баланс 1000

            // Act
            account.ApplyInterest();

            // Assert
            // Ожидаем 2% от 1000 = 20 у.е.
            var expectedInterest = 20m;
            var expectedBalance = 1020m;

            account.Balance.Should().Be(expectedBalance);

            // Проверяем, что создалась транзакция типа Interest с правильной суммой
            account.Transactions.Should().Contain(t =>
                t.Type == TransactionType.Interest &&
                t.Amount == expectedInterest
            );
        }

        [Fact]
        public void BusinessAccount_Withdraw_WithinLargeLimit_ShouldAllowOverdraft()
        {
            // Arrange
            var account = new BusinessAccount("Business Corp");
            account.Deposit(1000m);

            // Act
            // Снимаем 5000 при балансе 1000. Итого баланс должен стать -4000.
            // Это разрешено, так как лимит 5000.
            account.Withdraw(5000m);

            // Assert
            account.Balance.Should().Be(-4000m);
        }
        [Fact]
        public void BusinessAccount_ApplyMonthlyFee_ShouldDecreaseBalanceBy20()
        {
            // Arrange
            var account = new BusinessAccount("Business Corp");
            account.Deposit(100m);

            // Act
            account.ApplyMonthlyFee();

            // Assert
            account.Balance.Should().Be(80m); // 100 - 20 = 80

            // Проверяем наличие транзакции типа Fee
            account.Transactions.Should().Contain(t =>
            t.Type == TransactionType.Fee &&
            Math.Abs(t.Amount) == 20m);
        }

    }
}
