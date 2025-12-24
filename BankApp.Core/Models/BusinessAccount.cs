using System;

namespace BankApp.Core.Models
{
    public class BusinessAccount : Account
    {
        private const decimal MonthlyFee = 20m;
        private const decimal LargeOverdraftLimit = 5000m;

        public BusinessAccount(string ownerName) : base(ownerName)
        {
        }

        public override void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAmountException(); // Используем новое исключение
            
             UpdateBalance(amount, TransactionType.Deposit, $"Deposited {amount}.");
        }

        public override void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAmountException(); // Используем новое исключение

            // Высокий лимит овердрафта для бизнеса
            if (Balance + LargeOverdraftLimit >= amount)
            {
               UpdateBalance(-amount, TransactionType.Withdrawal, $"Withdrew {amount}.");
            }
            else
            {
                throw new InvalidAmountException("Withdrawal amount exceeds large business overdraft limit.");
            }
        }

        // Специфичное поведение: списание ежемесячной платы
        public void ApplyMonthlyFee()
        {
            UpdateBalance(-MonthlyFee, TransactionType.Fee, $"Monthly fee of {MonthlyFee} applied. New balance: {Balance}");
            
        }
    }
}
