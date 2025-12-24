using System;

namespace BankApp.Core.Models
{
    public static class AccountFactory
    {
        public static Account CreateAccount(string accountType, string ownerName, decimal initialDeposit = 0)
        {
            Account newAccount;
            switch (accountType.ToLower())
            {
                case "checking":
                    newAccount = new CheckingAccount(ownerName);
                    break;
                case "savings":
                    newAccount = new SavingsAccount(ownerName);
                    break;
                case "business":
                    newAccount = new BusinessAccount(ownerName);
                    break;
                default:
                    throw new ArgumentException("Invalid account type specified.", nameof(accountType));
            }

            if (initialDeposit > 0)
            {
                // Используем доменный метод Deposit для валидации и обновления баланса
                newAccount.Deposit(initialDeposit); 
            }
            else if (initialDeposit < 0)
            {
                throw new InvalidAmountException("Initial deposit cannot be negative.");
            }

            return newAccount;
        }
    }
}
