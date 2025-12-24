using System;

namespace BankApp.Core.Models
{
    public class SavingsAccount : Account
    {
         private readonly IInterestCalculationStrategy _interestStrategy;

        public SavingsAccount(string ownerName) : base(ownerName)
        {
            _interestStrategy = new StandardInterestStrategy(); 
        }

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

            // Накопительный счет не позволяет уходить в минус (нет овердрафта)
            if (Balance >= amount)
            {
               UpdateBalance(-amount, TransactionType.Withdrawal, $"Withdrew {amount}.");
            }
            else
            {
                throw new InsufficientFundsException("Insufficient funds. Savings accounts do not allow overdrafts.");
            }
        }

        // Специфичное поведение: начисление процентов
        public void ApplyInterest()
        {
            decimal interestAmount = _interestStrategy.CalculateInterest(Balance);
            if (interestAmount > 0)
            {
                UpdateBalance(interestAmount, TransactionType.Interest, $"Interest applied {interestAmount}. New balance: {Balance}");
                }
        }
    }
}
