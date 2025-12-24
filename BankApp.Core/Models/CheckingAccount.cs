// CheckingAccount.cs (обновленный код)

using System;

namespace BankApp.Core.Models
{
    public class CheckingAccount : Account
    {
        private const decimal OverdraftLimit = 500m;

        public CheckingAccount(string ownerName) : base(ownerName) { }

        public override void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAmountException(); // Используем новое исключение
            
            // Используем обновленный метод с типом транзакции и описанием
            UpdateBalance(amount, TransactionType.Deposit, $"Deposited {amount}.");
        }

        public override void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAmountException(); // Используем новое исключение

            if (Balance + OverdraftLimit >= amount)
            {
                // Используем обновленный метод
                UpdateBalance(-amount, TransactionType.Withdrawal, $"Withdrew {amount}.");
            }
            else
            {
                throw new InsufficientFundsException("Withdrawal amount exceeds overdraft limit."); // Используем новое исключение
            }
        }
    }
}
