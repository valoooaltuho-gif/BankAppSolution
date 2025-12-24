// BankApp.Core/Account.cs (ВОССТАНОВЛЕННЫЙ код)

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Оставьте этот using

namespace BankApp.Core.Models
{
    // Оставьте атрибуты полиморфизма
    [JsonDerivedType(typeof(CheckingAccount), "checking")]
    [JsonDerivedType(typeof(SavingsAccount), "savings")]
    [JsonDerivedType(typeof(BusinessAccount), "business")]
    public abstract class Account
    {
        public Guid Id { get; set; }
        public string OwnerName { get; set; }

        // ВОССТАНОВИТЕ: Свойство с публичным геттером и приватным сеттером
        public decimal Balance { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Transaction> Transactions { get; set; } // Оставьте List<Transaction>

        private Account()
        {
            OwnerName = string.Empty;
            Transactions = new List<Transaction>();
        }

        [JsonConstructor]
        protected Account(Guid id, string ownerName, decimal balance, DateTime createdAt, List<Transaction> transactions)
        {
            Id = id;
            OwnerName = ownerName;
            Balance = balance;
            CreatedAt = createdAt;
            Transactions = transactions ?? new List<Transaction>();
        }

        protected Account(string ownerName)
        {
            Id = Guid.NewGuid();
            OwnerName = ownerName;
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
            Transactions = new List<Transaction>();
        }

        public abstract void Deposit(decimal amount);
        public abstract void Withdraw(decimal amount);

        // ВОССТАНОВИТЕ: Метод для изменения баланса и добавления транзакции
        protected void UpdateBalance(decimal amount, TransactionType type, string description)
        {
            Balance += amount; // Баланс изменяется здесь
            var transaction = new Transaction(
                Id: Guid.NewGuid(),
                Date: DateTime.UtcNow,
                Type: type,
                Amount: Math.Abs(amount),
                BalanceAfter: Balance,
                Description: description
            );
            Transactions.Add(transaction);
        }
    }
}
